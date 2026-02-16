using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Folios;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class FolioService : IFolioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FolioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseObjectDto<GuestFolioSummaryDto>> CreateFolioForReservationAsync(int reservationId)
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

        public async Task<ResponseObjectDto<FolioTransactionDto>> AddTransactionAsync(CreateTransactionDto dto)
        {
            if (dto.Amount <= 0)
            {
                return Failure<FolioTransactionDto>("Amount must be greater than zero", 400);
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

            var transaction = new FolioTransaction
            {
                FolioId = folio.Id,
                Date = DateTime.UtcNow,
                Type = dto.Type,
                Amount = signedAmount,
                Description = dto.Description,
                ReferenceNo = dto.ReferenceNo,
                IsVoided = false
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
                IsVoided = false
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

