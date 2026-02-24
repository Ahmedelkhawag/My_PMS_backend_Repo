using AutoMapper;
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
using Microsoft.Extensions.Logging;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class FolioService : IFolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FolioService> _logger;
        private readonly IMapper _mapper;

        public FolioService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<FolioService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mapper = mapper;
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
                Data = _mapper.Map<GuestFolioSummaryDto>(folio)
            };
        }

        public async Task<ResponseObjectDto<FolioDetailsDto>> GetFolioDetailsAsync(int reservationId)
        {
            var reservation = await _unitOfWork.Reservations.GetQueryable()
                .AsNoTracking()
                .Include(r => r.Guest)
                .Include(r => r.RoomType)
                .Include(r => r.Room)
                .Include(r => r.Company)
                .Include(r => r.RatePlan)
                .Include(r => r.BookingSource)
                .Include(r => r.MealPlan)
                .Include(r => r.MarketSegment)
                .Include(r => r.Services)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return Failure<FolioDetailsDto>("Reservation not found", 404);
            }

            var folio = await _unitOfWork.GuestFolios
                .GetQueryable()
                .AsNoTracking()
                .Include(f => f.Transactions)
                .FirstOrDefaultAsync(f => f.ReservationId == reservationId);

            if (folio == null)
            {
                return Failure<FolioDetailsDto>("Folio not found for this reservation", 404);
            }

            var orderedTransactions = folio.Transactions
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Select(t => _mapper.Map<FolioTransactionDto>(t))
                .ToList();

            var reservationDto = new PMS.Application.DTOs.Reservations.ReservationDto
            {
                Id = reservation.Id,
                ReservationNumber = reservation.ReservationNumber,
                GuestId = reservation.GuestId,
                GuestName = reservation.Guest?.FullName,
                GuestPhone = reservation.Guest?.PhoneNumber,
                GuestEmail = reservation.Guest?.Email,
                GuestNationalId = reservation.Guest?.NationalId,
                RoomTypeId = reservation.RoomTypeId,
                RoomTypeName = reservation.RoomType?.Name,
                RoomId = reservation.RoomId,
                RoomNumber = reservation.Room?.RoomNumber,
                CompanyId = reservation.CompanyId,
                CompanyName = reservation.Company?.Name,
                RatePlanId = reservation.RatePlanId,
                RatePlanName = reservation.RatePlan?.Name,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Nights = (reservation.CheckOutDate - reservation.CheckInDate).Days > 0 ? (reservation.CheckOutDate - reservation.CheckInDate).Days : 1,
                RateCode = reservation.RateCode,
                MealPlan = reservation.MealPlan?.Name,
                Source = reservation.BookingSource?.Name,
                BookingSourceId = reservation.BookingSourceId,
                MarketSegmentId = reservation.MarketSegmentId,
                MealPlanId = reservation.MealPlanId,
                NightlyRate = reservation.NightlyRate,
                TotalAmount = reservation.TotalAmount,
                ServicesAmount = reservation.ServicesAmount,
                DiscountAmount = reservation.DiscountAmount,
                TaxAmount = reservation.TaxAmount,
                GrandTotal = reservation.GrandTotal,
                IsNoExtend = reservation.IsNoExtend,
                IsConfidentialRate = reservation.IsConfidentialRate,
                Status = reservation.Status.ToString(),
                Notes = reservation.Notes,
                ExternalReference = reservation.ExternalReference,
                CarPlate = reservation.CarPlate,
                PurposeOfVisit = reservation.PurposeOfVisit,
                MarketSegment = reservation.MarketSegment?.Name,
                Services = reservation.Services?.Select(s => new PMS.Application.DTOs.Reservations.ReservationServiceDto
                {
                    ServiceName = s.ServiceName,
                    Price = s.Price,
                    Quantity = s.Quantity,
                    IsPerDay = s.IsPerDay,
                    Total = s.TotalServicePrice,
                    ExtraServiceId = null
                }).ToList() ?? new List<PMS.Application.DTOs.Reservations.ReservationServiceDto>()
            };

            var expectedRemainingBalance = reservation.GrandTotal - folio.TotalPayments;

            var details = new FolioDetailsDto
            {
                ReservationId = folio.ReservationId,
                FolioId = folio.Id,
                TotalCharges = folio.TotalCharges,
                TotalPayments = folio.TotalPayments,
                Balance = folio.Balance,
                ExpectedRemainingBalance = expectedRemainingBalance,
                IsActive = folio.IsActive,
                Currency = folio.Currency,
                ReservationDetails = reservationDto,
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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await ProcessTransactionInternalAsync(dto);
                if (result.IsSuccess)
                {
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error adding transaction for reservation {ReservationId}", dto.ReservationId);
                return Failure<FolioTransactionDto>("An error occurred while adding the transaction.", 500);
            }
        }

        public async Task<ResponseObjectDto<FolioTransactionDto>> AddTransactionWithoutCommitAsync(CreateTransactionDto dto)
        {
            try
            {
                var result = await ProcessTransactionInternalAsync(dto);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding transaction without commit for reservation {ReservationId}", dto.ReservationId);
                return Failure<FolioTransactionDto>("An error occurred while adding the transaction.", 500);
            }
        }

        private async Task<ResponseObjectDto<FolioTransactionDto>> ProcessTransactionInternalAsync(CreateTransactionDto dto)

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
                await _unitOfWork.GuestFolios.GetQueryable()
                    .Where(f => f.Id == folio.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(f => f.TotalCharges, f => f.TotalCharges + signedAmount)
                        .SetProperty(f => f.Balance, f => f.Balance + signedAmount));
                
                folio.TotalCharges += signedAmount;
                folio.Balance += signedAmount;
            }
            else
            {
                await _unitOfWork.GuestFolios.GetQueryable()
                    .Where(f => f.Id == folio.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(f => f.TotalPayments, f => f.TotalPayments + signedAmount)
                        .SetProperty(f => f.Balance, f => f.Balance - signedAmount));

                folio.TotalPayments += signedAmount;
                folio.Balance -= signedAmount;
            }

            return new ResponseObjectDto<FolioTransactionDto>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Transaction added successfully",
            Data = _mapper.Map<FolioTransactionDto>(transaction)
            };
        }

        public async Task<ResponseObjectDto<FolioTransactionDto>> VoidTransactionAsync(int transactionId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var transaction = await _unitOfWork.FolioTransactions
                    .GetQueryable()
                    .Include(t => t.Folio)
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<FolioTransactionDto>("Transaction not found", 404);
                }

                if (transaction.Description != null && transaction.Description.StartsWith("VOID:"))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<FolioTransactionDto>("Cannot void a system-generated reversal transaction.", 400);
                }

                // شيلنا الـ if (transaction.IsVoided) من هنا عشان هنعملها بشكل آمن تحت

                var folio = transaction.Folio;
                if (folio == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<FolioTransactionDto>("Associated folio not found", 404);
                }

                if (!folio.IsActive)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<FolioTransactionDto>("Cannot void a transaction on a closed folio.", 400);
                }

                int? activeShiftId = transaction.ShiftId;

                if (IsCredit(transaction.Type))
                {
                    var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrWhiteSpace(currentUserId))
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Failure<FolioTransactionDto>("Unauthorized: cannot determine current user.", 401);
                    }

                    var activeShift = await _unitOfWork.EmployeeShifts
                        .GetQueryable()
                        .AsNoTracking()
                        .OrderByDescending(s => s.StartedAt)
                        .FirstOrDefaultAsync(s => s.EmployeeId == currentUserId && !s.IsClosed);

                    if (activeShift == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Failure<FolioTransactionDto>("No active shift found. Please open a shift before voiding a transaction.", 400);
                    }

                    activeShiftId = activeShift.Id;
                }

                var currentBusinessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

                // 🚨 التعديل السحري: Voiding + Locking (عشان نمنع الـ Race Condition)
                var rowsAffected = await _unitOfWork.FolioTransactions.GetQueryable()
                    .Where(t => t.Id == transactionId && !t.IsVoided)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsVoided, true));

                if (rowsAffected == 0)
                {
                    // لو رجعت صفر يبقى الحركة دي اتلغت أو اتنقلت في نفس اللحظة من ريكويست تاني
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<FolioTransactionDto>("Transaction is already voided or processed by another request.", 409);
                }

                // شيلنا السطر القديم بتاع: transaction.IsVoided = true;

                var isDebit = IsDebit(transaction.Type);
                var signedReverseAmount = -transaction.Amount;

                var reversal = new FolioTransaction
                {
                    FolioId = transaction.FolioId,
                    Date = DateTime.UtcNow,
                    BusinessDate = currentBusinessDate,
                    Type = transaction.Type,
                    Amount = signedReverseAmount,
                    Description = $"VOID: {transaction.Description}",
                    ReferenceNo = transaction.ReferenceNo,
                    IsVoided = false, // ✅ الكارثة اتصلحت: الحركة دي لازم تفضل عايشة
                    ShiftId = activeShiftId
                };

                await _unitOfWork.FolioTransactions.AddAsync(reversal);

                // ... بقيت الكود بتاع الـ ExecuteUpdateAsync للـ Folio زي ما هو بالظبط ...
                if (isDebit)
                {
                    await _unitOfWork.GuestFolios.GetQueryable()
                        .Where(f => f.Id == folio.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.TotalCharges, f => f.TotalCharges + signedReverseAmount)
                            .SetProperty(f => f.Balance, f => f.Balance + signedReverseAmount));
                }
                else
                {
                    await _unitOfWork.GuestFolios.GetQueryable()
                        .Where(f => f.Id == folio.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.TotalPayments, f => f.TotalPayments + signedReverseAmount)
                            .SetProperty(f => f.Balance, f => f.Balance - signedReverseAmount));
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseObjectDto<FolioTransactionDto>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Transaction voided successfully",
                    Data = _mapper.Map<FolioTransactionDto>(reversal)
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                // ... (اللوج هنا زي ما كان)
                return Failure<FolioTransactionDto>("An internal error occurred while processing the transaction.", 500);
            }
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

                var paymentResult = await ProcessTransactionInternalAsync(paymentDto);
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

                    var discountResult = await ProcessTransactionInternalAsync(discountDto);
                    if (!discountResult.IsSuccess)
                    {
                        // لو الخصم فشل (مثلاً نسي يكتب السبب)، نلغي الدفع وكل اللي حصل
                        await _unitOfWork.RollbackTransactionAsync();
                        return Failure<bool>($"Discount failed: {discountResult.Message}", 400);
                    }
                }

                // 4. لو كله تمام، نـ Commit التغييرات للداتابيز بأمان
                await _unitOfWork.CompleteAsync();
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
                _logger.LogError(ex, "A critical error occurred while processing payment and optional discount for reservation {ReservationId}.", dto.ReservationId);
                return Failure<bool>("An internal error occurred while processing the transaction. Please try again or contact support.", 500);
            }
        }

        public async Task<ResponseObjectDto<FolioTransactionDto>> RefundTransactionAsync(int originalTransactionId, decimal refundAmount, string reason, string userId)
        {
            await _unitOfWork.BeginTransactionAsync(); // 1. فتحنا الترانزاكشن
            try
            {
                if (refundAmount <= 0)
                {
                    return Failure<FolioTransactionDto>("Refund amount must be greater than zero.", 400);
                }

                var originalTransaction = await _unitOfWork.FolioTransactions
                    .GetQueryable()
                    .Include(t => t.Folio)
                    .FirstOrDefaultAsync(t => t.Id == originalTransactionId);

                if (originalTransaction == null)
                {
                    return Failure<FolioTransactionDto>("Original transaction not found.", 404);
                }

                var isPayment = originalTransaction.Type == TransactionType.CashPayment ||
                                originalTransaction.Type == TransactionType.CardPayment ||
                                originalTransaction.Type == TransactionType.BankTransferPayment ||
                                originalTransaction.Type == TransactionType.CityLedgerPayment;

                if (!isPayment)
                {
                    return Failure<FolioTransactionDto>("Original transaction is not a payment and cannot be refunded.", 400);
                }

                if (refundAmount > originalTransaction.Amount)
                {
                    return Failure<FolioTransactionDto>("Refund amount cannot be greater than the original transaction amount.", 400);
                }

                // Calculate total refunded amount for the original transaction
                var previouslyRefundedAmount = await _unitOfWork.FolioTransactions
                    .GetQueryable()
                    .Where(t => t.Type == TransactionType.Refund &&
                                !t.IsVoided &&
                                t.ReferenceNo != null &&
                                t.ReferenceNo.Contains(originalTransactionId.ToString()))
                    .SumAsync(t => t.Amount);

                // Note: Refund amounts are negative, so we subtract it from the previous refunds or use Math.Abs
                var absolutePreviouslyRefundedAmount = Math.Abs(previouslyRefundedAmount);

                if (refundAmount > (originalTransaction.Amount - absolutePreviouslyRefundedAmount))
                {
                    return Failure<FolioTransactionDto>($"Refund amount exceeds the remaining refundable amount. Remaining refundable amount: {originalTransaction.Amount - absolutePreviouslyRefundedAmount}.", 400);
                }

                var folio = originalTransaction.Folio;
                if (folio == null)
                {
                    return Failure<FolioTransactionDto>("Associated folio not found.", 404);
                }

                if (!folio.IsActive)
                {
                    return Failure<FolioTransactionDto>("Cannot add refund to a closed folio.", 400);
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Failure<FolioTransactionDto>("Unauthorized: cannot determine current user.", 401);
                }

                var activeShift = await _unitOfWork.EmployeeShifts
                    .GetQueryable()
                    .AsNoTracking()
                    .OrderByDescending(s => s.StartedAt)
                    .FirstOrDefaultAsync(s => s.EmployeeId == userId && !s.IsClosed);

                if (activeShift == null)
                {
                    return Failure<FolioTransactionDto>("No active shift found. Please open a shift before taking refunds.", 400);
                }

                var currentBusinessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

                var refundTransaction = new FolioTransaction
                {
                    FolioId = folio.Id,
                    Date = DateTime.UtcNow,
                    BusinessDate = currentBusinessDate,
                    Type = TransactionType.Refund,
                    Amount = -refundAmount,
                    Description = string.IsNullOrWhiteSpace(reason) ? $"Refund for transaction {originalTransactionId}" : reason,
                    ReferenceNo = string.IsNullOrWhiteSpace(originalTransaction.ReferenceNo)
                                    ? $"REF-{originalTransactionId}"
                                    : $"{originalTransaction.ReferenceNo}-REF-{originalTransactionId}",
                    IsVoided = false,
                    ShiftId = activeShift.Id
                };

                await _unitOfWork.FolioTransactions.AddAsync(refundTransaction);

                await _unitOfWork.GuestFolios.GetQueryable()
                    .Where(f => f.Id == folio.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(f => f.TotalPayments, f => f.TotalPayments - refundAmount)
                        .SetProperty(f => f.Balance, f => f.Balance + refundAmount));

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseObjectDto<FolioTransactionDto>
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Refund processed successfully.",
                    Data = _mapper.Map<FolioTransactionDto>(refundTransaction)
                };
            }
            catch (Exception ex)
            {

                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "A critical error occurred while processing refund for original transaction {OriginalTransactionId}.", originalTransactionId);
                return Failure<FolioTransactionDto>("An internal error occurred while processing the refund. Please try again or contact support.", 500);
            }
        }
           
       

        public async Task<ResponseObjectDto<bool>> TransferTransactionAsync(int transactionId, int targetReservationId, string userId, string reason)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var originalTransaction = await _unitOfWork.FolioTransactions
                    .GetQueryable()
                    .Include(t => t.Folio)
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (originalTransaction == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Transaction not found.", 404);
                }

                if (originalTransaction.Description != null && originalTransaction.Description.StartsWith("Transfer Out"))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Cannot void a transfer transaction directly. Reverse the transfer instead.", 400);
                }

                if (originalTransaction.IsVoided)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Transaction is already voided.", 400);
                }

                var sourceFolio = originalTransaction.Folio;
                if (sourceFolio == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Source folio not found.", 404);
                }

                var targetFolio = await _unitOfWork.GuestFolios
                    .GetQueryable()
                    .FirstOrDefaultAsync(f => f.ReservationId == targetReservationId);

                if (targetFolio == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Target folio not found.", 404);
                }

                if (!targetFolio.IsActive)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Target folio is closed.", 400);
                }

                if (sourceFolio.Id == targetFolio.Id)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Cannot transfer to the same folio.", 400);
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Unauthorized: cannot determine current user.", 401);
                }

                var activeShift = await _unitOfWork.EmployeeShifts
                    .GetQueryable()
                    .AsNoTracking()
                    .OrderByDescending(s => s.StartedAt)
                    .FirstOrDefaultAsync(s => s.EmployeeId == userId && !s.IsClosed);

                if (activeShift == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("No active shift found. Please open a shift before transferring a transaction.", 400);
                }

                var currentBusinessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

                // Step 3: Source Folio Logic (The Outgoing)
                var rowsAffected = await _unitOfWork.FolioTransactions.GetQueryable()
                                  .Where(t => t.Id == transactionId && !t.IsVoided)
                                  .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsVoided, true));

                if (rowsAffected == 0)
                {
                    // لو مفيش صفوف اتعدلت، يبقى الترانزاكشن دي اتلغت أو اتنقلت في نفس اللحظة من ريكويست تاني
                    await _unitOfWork.RollbackTransactionAsync();
                    return Failure<bool>("Transaction is already transferred or voided by another process.", 409); // 409 Conflict
                }
                // originalTransaction.IsVoided = true;

                var isDebit = IsDebit(originalTransaction.Type);
                var signedReverseAmount = -originalTransaction.Amount;

                var reversalDescription = string.IsNullOrWhiteSpace(reason)
                    ? $"Transfer Out to Res #{targetReservationId}: {originalTransaction.Description}"
                    : $"Transfer Out - {reason}";

                var reversal = new FolioTransaction
                {
                    FolioId = originalTransaction.FolioId,
                    Date = DateTime.UtcNow,
                    BusinessDate = currentBusinessDate,
                    Type = originalTransaction.Type,
                    Amount = signedReverseAmount,
                    Description = reversalDescription,
                    ReferenceNo = originalTransaction.ReferenceNo,
                    IsVoided = false,
                    ShiftId = activeShift.Id
                };

                await _unitOfWork.FolioTransactions.AddAsync(reversal);

                if (isDebit)
                {
                    await _unitOfWork.GuestFolios.GetQueryable()
                        .Where(f => f.Id == sourceFolio.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.TotalCharges, f => f.TotalCharges + signedReverseAmount)
                            .SetProperty(f => f.Balance, f => f.Balance + signedReverseAmount));
                }
                else
                {
                    await _unitOfWork.GuestFolios.GetQueryable()
                        .Where(f => f.Id == sourceFolio.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.TotalPayments, f => f.TotalPayments + signedReverseAmount)
                            .SetProperty(f => f.Balance, f => f.Balance - signedReverseAmount));
                }

                // Step 4: Target Folio Logic (The Incoming)
                var targetType = isDebit ? TransactionType.TransferDebit : TransactionType.TransferCredit;

                var targetDescription = $"Transferred from Res #{sourceFolio.ReservationId} - {originalTransaction.Description}";

                var newTransaction = new FolioTransaction
                {
                    FolioId = targetFolio.Id,
                    Date = DateTime.UtcNow,
                    BusinessDate = currentBusinessDate,
                    Type = targetType,
                    Amount = originalTransaction.Amount,
                    Description = targetDescription,
                    ReferenceNo = originalTransaction.ReferenceNo,
                    IsVoided = false,
                    ShiftId = activeShift.Id
                };

                await _unitOfWork.FolioTransactions.AddAsync(newTransaction);

                if (isDebit)
                {
                    await _unitOfWork.GuestFolios.GetQueryable()
                        .Where(f => f.Id == targetFolio.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.TotalCharges, f => f.TotalCharges + newTransaction.Amount)
                            .SetProperty(f => f.Balance, f => f.Balance + newTransaction.Amount));
                }
                else
                {
                    await _unitOfWork.GuestFolios.GetQueryable()
                        .Where(f => f.Id == targetFolio.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.TotalPayments, f => f.TotalPayments + newTransaction.Amount)
                            .SetProperty(f => f.Balance, f => f.Balance - newTransaction.Amount));
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseObjectDto<bool>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Transaction transferred successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "A critical error occurred while transferring transaction {TransactionId} to reservation {TargetReservationId}.", transactionId, targetReservationId);
                return Failure<bool>("An internal error occurred while processing the transaction. Please try again or contact support.", 500);
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

