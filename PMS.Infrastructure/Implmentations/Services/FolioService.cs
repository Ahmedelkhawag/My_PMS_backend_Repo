using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Folios;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class FolioService : IFolioService
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

        public FolioService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseObjectDto<GuestFolioSummaryDto>> GetFolioSummaryAsync(int reservationId)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return Failure<GuestFolioSummaryDto>("Reservation not found", 404);
            }

            var folio = await _unitOfWork.GuestFolios
                .FindAsync(f => f.ReservationId == reservationId);

            if (folio == null)
            {
                return Failure<GuestFolioSummaryDto>("Folio not found for this reservation", 404);
            }

            return new ResponseObjectDto<GuestFolioSummaryDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Folio retrieved successfully",
                Data = MapToSummaryDto(folio)
            };
        }

        public async Task<ResponseObjectDto<FolioDetailsDto>> GetFolioDetailsAsync(int reservationId)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return Failure<FolioDetailsDto>("Reservation not found", 404);
            }

            var folio = await _unitOfWork.GuestFolios
                .GetQueryable()
                .Include(f => f.Transactions)
                .FirstOrDefaultAsync(f => f.ReservationId == reservationId);

            if (folio == null)
            {
                return Failure<FolioDetailsDto>("Folio not found for this reservation", 404);
            }

            var orderedTransactions = folio.Transactions
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Select(MapToTransactionDto)
                .ToList();

            var details = new FolioDetailsDto
            {
                ReservationId = folio.ReservationId,
                FolioId = folio.Id,
                TotalCharges = folio.TotalCharges,
                TotalPayments = folio.TotalPayments,
                Balance = folio.Balance,
                IsActive = folio.IsActive,
                Currency = folio.Currency,
                Transactions = orderedTransactions
            };

            return new ResponseObjectDto<FolioDetailsDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Folio details retrieved successfully",
                Data = details
            };
        }

        public async Task<ResponseObjectDto<bool>> CloseFolioAsync(int reservationId)
        {
            var folio = await _unitOfWork.GuestFolios
                .GetQueryable()
                .FirstOrDefaultAsync(f => f.ReservationId == reservationId);

            if (folio == null)
            {
                return Failure<bool>("Folio not found for this reservation", 404);
            }

            if (!folio.IsActive)
            {
                return Failure<bool>("Folio is already closed.", 400);
            }

            if (folio.Balance != 0m)
            {
                var formattedBalance = folio.Balance.ToString("F2");
                return Failure<bool>($"Cannot close folio. Guest still has an outstanding balance of {formattedBalance}.", 400);
            }

            folio.IsActive = false;
            _unitOfWork.GuestFolios.Update(folio);

            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Folio closed successfully",
                Data = true
            };
        }

        public async Task<ResponseObjectDto<FolioTransactionDto>> AddTransactionAsync(CreateTransactionDto dto)
        {
            if (dto.Amount <= 0)
            {
                return Failure<FolioTransactionDto>("Amount must be greater than zero", 400);
            }

            if (dto.Type == TransactionType.CardPayment || dto.Type == TransactionType.BankTransferPayment)
            {
                if (string.IsNullOrWhiteSpace(dto.ReferenceNo))
                {
                    return Failure<FolioTransactionDto>("Reference number is required for electronic payments.", 400);
                }
            }

            if (dto.Type == TransactionType.Discount)
            {
                if (string.IsNullOrWhiteSpace(dto.DiscountReason))
                {
                    return Failure<FolioTransactionDto>("A reason must be provided for every discount.", 400);
                }
            }

            var reservation = await _unitOfWork.Reservations.GetByIdAsync(dto.ReservationId);
            if (reservation == null)
            {
                return Failure<FolioTransactionDto>("Reservation not found", 404);
            }

            var folio = await _unitOfWork.GuestFolios
                .FindAsync(f => f.ReservationId == dto.ReservationId);

            if (folio == null)
            {
                return Failure<FolioTransactionDto>("Folio not found for this reservation", 404);
            }

            if (!folio.IsActive)
            {
                return Failure<FolioTransactionDto>("Cannot add transaction to a closed folio", 400);
            }

            var isDebit = IsDebit(dto.Type);
            var signedAmount = dto.Amount;

            if (!isDebit && !IsCredit(dto.Type))
            {
                return Failure<FolioTransactionDto>("Unsupported transaction type", 400);
            }

			// Payment/Refund operations must be tied to an active shift.
			// In this codebase, payments are credit transactions (20-29). Refunds are represented as negative payment amounts (e.g. void reversal).
			int? activeShiftId = null;
			if (IsCredit(dto.Type))
			{
				var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrWhiteSpace(currentUserId))
				{
					return Failure<FolioTransactionDto>("Unauthorized: cannot determine current user.", 401);
				}

				var activeShift = await _unitOfWork.EmployeeShifts
					.GetQueryable()
					.AsNoTracking()
					.OrderByDescending(s => s.StartedAt)
					.FirstOrDefaultAsync(s => s.EmployeeId == currentUserId && !s.IsClosed);

				if (activeShift == null)
				{
					return Failure<FolioTransactionDto>("No active shift found. Please open a shift before taking payments/refunds.", 400);
				}

				activeShiftId = activeShift.Id;
			}

			// Use the current BusinessDate for all financial postings.
			var currentBusinessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

            var transaction = new FolioTransaction
            {
                FolioId = folio.Id,
                Date = DateTime.UtcNow,
				BusinessDate = currentBusinessDate,
                Type = dto.Type,
                Amount = signedAmount,
                Description = dto.Description,
                ReferenceNo = dto.ReferenceNo,
                DiscountReason = dto.DiscountReason,
                IsVoided = false,
				ShiftId = activeShiftId
            };

            await _unitOfWork.FolioTransactions.AddAsync(transaction);

            if (isDebit)
            {
                folio.TotalCharges += signedAmount;
            }
            else
            {
                folio.TotalPayments += signedAmount;
            }

            folio.Balance = folio.TotalCharges - folio.TotalPayments;

            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<FolioTransactionDto>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Transaction added successfully",
                Data = MapToTransactionDto(transaction)
            };
        }

        public async Task<ResponseObjectDto<FolioTransactionDto>> VoidTransactionAsync(int transactionId)
        {
            var transaction = await _unitOfWork.FolioTransactions
                .GetQueryable()
                .Include(t => t.Folio)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
            {
                return Failure<FolioTransactionDto>("Transaction not found", 404);
            }

            if (transaction.IsVoided)
            {
                return Failure<FolioTransactionDto>("Transaction is already voided", 400);
            }

            var folio = transaction.Folio;
            if (folio == null)
            {
                return Failure<FolioTransactionDto>("Associated folio not found", 404);
            }

            transaction.IsVoided = true;

            var isDebit = IsDebit(transaction.Type);
            var signedReverseAmount = -transaction.Amount;

            var reversal = new FolioTransaction
            {
                FolioId = transaction.FolioId,
                Date = DateTime.UtcNow,
                Type = transaction.Type,
                Amount = signedReverseAmount,
                Description = $"VOID: {transaction.Description}",
                ReferenceNo = transaction.ReferenceNo,
                IsVoided = false,
				ShiftId = transaction.ShiftId
            };

            await _unitOfWork.FolioTransactions.AddAsync(reversal);

            if (isDebit)
            {
                folio.TotalCharges += signedReverseAmount;
            }
            else
            {
                folio.TotalPayments += signedReverseAmount;
            }

            folio.Balance = folio.TotalCharges - folio.TotalPayments;

            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<FolioTransactionDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Transaction voided successfully",
                Data = MapToTransactionDto(reversal)
            };
        }


        public async Task<ResponseObjectDto<bool>> PostPaymentWithDiscountAsync(PostPaymentWithDiscountDto dto)
        {
            // 1. نفتح الـ Database Transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 2. نسجل عملية الدفع الأساسية
                var paymentDto = new CreateTransactionDto
                {
                    ReservationId = dto.ReservationId,
                    Type = dto.PaymentType,
                    Amount = dto.PaymentAmount,
                    Description = dto.PaymentDescription,
                    ReferenceNo = dto.ReferenceNo
                };

                var paymentResult = await AddTransactionAsync(paymentDto);
                if (!paymentResult.IsSuccess)
                {
                    // لو الدفع فشل (مثلاً نسي يكتب رقم الفيزا)، نلغي كل حاجة
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>($"Payment failed: {paymentResult.Message}", 400);
                }

                // 3. لو فيه خصم، نسجله كعملية تانية
                if (dto.ApplyDiscount)
                {
                    if (dto.DiscountAmount == null || dto.DiscountAmount <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Failure<bool>("Discount amount must be greater than zero when apply discount is checked.", 400);
                    }

                    var discountDto = new CreateTransactionDto
                    {
                        ReservationId = dto.ReservationId,
                        Type = TransactionType.Discount,
                        Amount = dto.DiscountAmount.Value,
                        Description = dto.DiscountDescription ?? "Discount Applied",
                        DiscountReason = dto.DiscountReason
                    };

                    var discountResult = await AddTransactionAsync(discountDto);
                    if (!discountResult.IsSuccess)
                    {
                        // لو الخصم فشل (مثلاً نسي يكتب السبب)، نلغي الدفع وكل اللي حصل
                        await _unitOfWork.RollbackTransactionAsync();
                        return Failure<bool>($"Discount failed: {discountResult.Message}", 400);
                    }
                }

                // 4. لو كله تمام، نـ Commit التغييرات للداتابيز بأمان
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseObjectDto<bool>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Payment and optional discount processed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                // لو ضرب أي Error فجأة، نرجع في كلامنا فوراً
                await _unitOfWork.RollbackTransactionAsync();
                return Failure<bool>("An error occurred while processing the payment.", 500);
            }
        }

        private static bool IsDebit(TransactionType type)
        {
            var code = (int)type;
            return code >= 10 && code <= 19;
        }

        private static bool IsCredit(TransactionType type)
        {
            var code = (int)type;
            return code >= 20 && code <= 29;
        }

        private static GuestFolioSummaryDto MapToSummaryDto(GuestFolio folio)
        {
            return new GuestFolioSummaryDto
            {
                ReservationId = folio.ReservationId,
                FolioId = folio.Id,
                TotalCharges = folio.TotalCharges,
                TotalPayments = folio.TotalPayments,
                Balance = folio.Balance,
                IsActive = folio.IsActive,
                Currency = folio.Currency
            };
        }

        private static FolioTransactionDto MapToTransactionDto(FolioTransaction transaction)
        {
            return new FolioTransactionDto
            {
                Id = transaction.Id,
                FolioId = transaction.FolioId,
                Date = transaction.Date,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Description = transaction.Description,
                ReferenceNo = transaction.ReferenceNo,
                IsVoided = transaction.IsVoided,
                CreatedBy = transaction.CreatedBy,
                CreatedAt = transaction.CreatedAt
            };
        }

        private static ResponseObjectDto<T> Failure<T>(string message, int statusCode)
        {
            return new ResponseObjectDto<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode
            };
        }
    }
}

