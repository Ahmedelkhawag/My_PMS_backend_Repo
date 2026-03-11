using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Entities.BackOffice.AR;
using PMS.Domain.Enums;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<CommissionService> _logger;

        public CommissionService(IUnitOfWork unitOfWork, IAccountingService accountingService, ILogger<CommissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _accountingService = accountingService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // CALCULATE
        // ─────────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<ApiResponse<bool>> CalculateForReservationAsync(int reservationId)
        {
            // 1. Load reservation + Company + GuestFolio → Transactions
            var reservation = await _unitOfWork.Reservations
                .GetQueryable()
                .AsNoTracking()
                .Include(r => r.Company)
                .Include(r => r.GuestFolio)
                    .ThenInclude(f => f.Transactions)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return new ApiResponse<bool>("Reservation not found.");

            // 2. Guard: company must have a positive commission rate
            if (reservation.CompanyId == null || reservation.Company == null)
                return new ApiResponse<bool>("Reservation is not linked to a company; no commission applicable.");

            var commissionRate = reservation.Company.CommissionRate ?? 0m;
            if (commissionRate <= 0m)
                return new ApiResponse<bool>("Company has no commission rate configured; skipping calculation.");

            // 3. Sum only un-voided RoomCharge transactions
            var transactions = reservation.GuestFolio?.Transactions
                ?? Enumerable.Empty<Domain.Entities.FolioTransaction>();

            var eligibleRevenue = transactions
                .Where(t => t.Type == TransactionType.RoomCharge && !t.IsVoided)
                .Sum(t => t.Amount);

            // 4. Calculate
            var commissionAmount = eligibleRevenue * (commissionRate / 100m);

            // 5. Upsert Draft record
            var existing = await _unitOfWork.TACommissionRecords
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.ReservationId == reservationId
                                       && c.Status == CommissionStatus.Draft);

            if (existing != null)
            {
                existing.EligibleRevenue  = eligibleRevenue;
                existing.CommissionRate   = commissionRate;
                existing.CommissionAmount = commissionAmount;
                _unitOfWork.TACommissionRecords.Update(existing);
            }
            else
            {
                await _unitOfWork.TACommissionRecords.AddAsync(new TACommissionRecord
                {
                    CompanyId        = reservation.CompanyId.Value,
                    ReservationId    = reservationId,
                    EligibleRevenue  = eligibleRevenue,
                    CommissionRate   = commissionRate,
                    CommissionAmount = commissionAmount,
                    Status           = CommissionStatus.Draft
                });
            }

            await _unitOfWork.CompleteAsync();

            return new ApiResponse<bool>(true,
                $"Commission of {commissionAmount:F2} calculated successfully for reservation #{reservationId}.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // GET PENDING
        // ─────────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<ApiResponse<IEnumerable<TACommissionRecordDto>>> GetPendingCommissionsAsync()
        {
            var records = await _unitOfWork.TACommissionRecords
                .GetQueryable()
                .AsNoTracking()
                .Include(c => c.Company)
                .Where(c => c.Status == CommissionStatus.Draft)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TACommissionRecordDto
                {
                    Id               = c.Id,
                    CompanyId        = c.CompanyId,
                    CompanyName      = c.Company.Name,
                    ReservationId    = c.ReservationId,
                    EligibleRevenue  = c.EligibleRevenue,
                    CommissionRate   = c.CommissionRate,
                    CommissionAmount = c.CommissionAmount,
                    Status           = c.Status,
                    JournalEntryId   = c.JournalEntryId
                })
                .ToListAsync();

            return new ApiResponse<IEnumerable<TACommissionRecordDto>>(records,
                $"{records.Count} pending commission(s) retrieved.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // APPROVE
        // ─────────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<ApiResponse<bool>> ApproveCommissionAsync(int recordId)
        {
            // 1. Load the record
            var record = await _unitOfWork.TACommissionRecords
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == recordId);

            if (record == null)
                return new ApiResponse<bool>("Commission record not found.");

            if (record.Status != CommissionStatus.Draft)
                return new ApiResponse<bool>($"Commission is already {record.Status} and cannot be approved again.");

            // 2. Wrap status update and GL posting in a transaction
            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }

            try
            {
                // Approve the record
                record.Status = CommissionStatus.Approved;
                _unitOfWork.TACommissionRecords.Update(record);
                await _unitOfWork.CompleteAsync();

                // 3. Delegate GL Posting to AccountingService
                var glResult = await _accountingService.PostTACommissionToGLAsync(record.Id);
                
                if (!glResult.Succeeded)
                {
                    if (isLocalTransaction)
                        await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>($"Failed to post commission to GL: {glResult.Message}");
                }

                if (isLocalTransaction)
                    await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, $"Commission #{recordId} approved and posted to GL successfully.");
            }
            catch (Exception ex)
            {
                if (isLocalTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                    
                _logger.LogError(ex, "Error approving commission #{RecordId}", recordId);
                throw;
            }
        }
    }
}
