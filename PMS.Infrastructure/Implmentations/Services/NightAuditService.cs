using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.Audit;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using PMS.Infrastructure.Context;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class NightAuditService : INightAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NightAuditService> _logger;

        public NightAuditService(
            IUnitOfWork unitOfWork,
            ILogger<NightAuditService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<AuditStatusDto>> GetCurrentStatusAsync()
        {
            var dto = new AuditStatusDto();

            var openBusinessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .OrderByDescending(b => b.Date)
                .FirstOrDefaultAsync(b => b.Status == BusinessDayStatus.Open);

            if (openBusinessDay != null)
            {
                dto.CurrentBusinessDate = openBusinessDay.Date;
                dto.IsOpen = true;
                dto.OpenSince = openBusinessDay.StartedAt;
            }
            else
            {
                // Fallback: last business day (closed) or today.
                var lastBusinessDay = await _unitOfWork.BusinessDays
                    .GetQueryable()
                    .AsNoTracking()
                    .OrderByDescending(b => b.Date)
                    .FirstOrDefaultAsync();

                dto.CurrentBusinessDate = lastBusinessDay?.Date ?? DateTime.UtcNow.Date;
                dto.IsOpen = false;
                dto.OpenSince = lastBusinessDay?.StartedAt ?? DateTime.UtcNow;
            }

            _logger.LogInformation("NightAuditStatus: BusinessDate={Date}, IsOpen={IsOpen}, OpenSince={OpenSince}",
                dto.CurrentBusinessDate, dto.IsOpen, dto.OpenSince);

            return new ApiResponse<AuditStatusDto>(dto, "Night audit status retrieved successfully.");
        }

        public async Task<ApiResponse<AuditResponseDto>> RunNightAuditAsync(string userId, bool force)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<AuditResponseDto>("Invalid user id.");
            }

            var currentBusinessDate = (await _unitOfWork.GetCurrentBusinessDateAsync()).Date;

            _logger.LogInformation("Starting night audit for BusinessDate={Date} by User={UserId}, Force={Force}",
                currentBusinessDate, userId, force);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var response = new AuditResponseDto
                {
                    NewBusinessDate = currentBusinessDate,
                    Message = string.Empty,
                    NoShowsMarked = 0,
                    ProcessedRooms = 0
                };

                var currentBusinessDay = await ((DbSet<BusinessDay>)_unitOfWork.BusinessDays.GetQueryable())
                    .FromSqlRaw("SELECT * FROM BusinessDays WITH (UPDLOCK, ROWLOCK) WHERE Status = {0}", (int)BusinessDayStatus.Open)
                    .FirstOrDefaultAsync();

                if (currentBusinessDay == null)
                {
                    _logger.LogWarning("Night audit aborted: no open BusinessDay found.");
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<AuditResponseDto>("No open BusinessDay found to run night audit.");
                }

                // Step A: Validation
                await ValidatePendingArrivalsAsync(currentBusinessDate, force);

                // Step B: Post Room Charges
                response.ProcessedRooms = await PostRoomChargesAsync(currentBusinessDate);

                // Step C: Handle No-Shows (Optimized)
                response.NoShowsMarked = await HandleNoShowsAsync(currentBusinessDate);

                // Step C.5: Force close all open employee shifts
                var openShiftsClosed = await _unitOfWork.EmployeeShifts
                    .GetQueryable()
                    .Where(s => !s.IsClosed)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsClosed, true)
                        .SetProperty(x => x.EndedAt, DateTime.UtcNow));

                _logger.LogInformation("Force-closed {Count} open employee shifts for Night Audit.", openShiftsClosed);

                // Step D: Roll Business Date
                response.NewBusinessDate = await RollBusinessDateAsync(currentBusinessDay, userId, currentBusinessDate);
                
                response.Message = $"Night audit completed successfully. New business date is {response.NewBusinessDate:yyyy-MM-dd}.";

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Night audit completed successfully for BusinessDate={OldDate}. NewBusinessDate={NewDate}, ProcessedRooms={ProcessedRooms}, NoShows={NoShows}.",
                    currentBusinessDate, response.NewBusinessDate, response.ProcessedRooms, response.NoShowsMarked);

                return new ApiResponse<AuditResponseDto>(response, response.Message);
            }
            catch (NightAuditValidationException ex)
            {
                _logger.LogWarning(ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
                return new ApiResponse<AuditResponseDto>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Night audit failed for BusinessDate={Date}. Rolling back transaction.", currentBusinessDate);
                await _unitOfWork.RollbackTransactionAsync();
                return new ApiResponse<AuditResponseDto>($"Night audit failed: {ex.Message}");
            }
        }

        private async Task ValidatePendingArrivalsAsync(DateTime currentBusinessDate, bool force)
        {
            if (force)
            {
                _logger.LogInformation("Night audit running in FORCE mode. Validation step skipped.");
                return;
            }

            var pendingOrConfirmedArrivals = await _unitOfWork.Reservations
                .GetQueryable()
                .AsNoTracking()
                .Where(r =>
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Confirmed) &&
                    r.CheckInDate.Date == currentBusinessDate.Date)
                .CountAsync();

            if (pendingOrConfirmedArrivals > 0)
            {
                throw new NightAuditValidationException(
                    $"Night audit blocked: {pendingOrConfirmedArrivals} reservations with status Pending/Confirmed still have Check-In = business date {currentBusinessDate:yyyy-MM-dd}.");
            }

            _logger.LogInformation("Night audit validation passed: no pending/confirmed arrivals for BusinessDate={Date}.",
                currentBusinessDate);
        }

        private async Task<int> PostRoomChargesAsync(DateTime currentBusinessDate)
        {
            var checkedInReservations = await _unitOfWork.Reservations
                .GetQueryable()
                .Where(r =>
                    r.Status == ReservationStatus.CheckIn &&
                    r.CheckOutDate.Date > currentBusinessDate)
                .ToListAsync();

            _logger.LogInformation("Found {Count} checked-in reservations eligible for room charges on BusinessDate={Date}.",
                checkedInReservations.Count, currentBusinessDate);

            if (!checkedInReservations.Any())
            {
                return 0;
            }

            var reservationIds = checkedInReservations.Select(r => r.Id).ToList();

            var folios = await _unitOfWork.GuestFolios
                .GetQueryable()
                .Where(f => reservationIds.Contains(f.ReservationId))
                .ToListAsync();

            var foliosByReservationId = folios.ToDictionary(f => f.ReservationId, f => f);
            var folioIds = folios.Select(f => f.Id).ToList();

            var existingRoomCharges = await _unitOfWork.FolioTransactions
                .GetQueryable()
                .AsNoTracking()
                .Where(t =>
                    t.Type == TransactionType.RoomCharge &&
                    t.BusinessDate == currentBusinessDate &&
                    folioIds.Contains(t.FolioId) &&
                    !t.IsVoided)
                .Select(t => t.FolioId)
                .ToListAsync();

            var foliosWithChargeToday = existingRoomCharges.ToHashSet();
            int processedRooms = 0;

            foreach (var reservation in checkedInReservations)
            {
                if (!foliosByReservationId.TryGetValue(reservation.Id, out var folio))
                {
                    _logger.LogWarning("Skipping room charge for ReservationId={ReservationId}: no folio found.",
                        reservation.Id);
                    continue;
                }

                if (foliosWithChargeToday.Contains(folio.Id))
                {
                    _logger.LogInformation(
                        "Skipping room charge for ReservationId={ReservationId}, FolioId={FolioId}: already charged for BusinessDate={Date}.",
                        reservation.Id, folio.Id, currentBusinessDate);
                    continue;
                }

                var chargeAmount = reservation.NightlyRate;

                var transactionEntity = new FolioTransaction
                {
                    FolioId = folio.Id,
                    Date = DateTime.UtcNow,
                    BusinessDate = currentBusinessDate,
                    Type = TransactionType.RoomCharge,
                    Amount = chargeAmount,
                    Description = $"Room Charge - {currentBusinessDate:yyyy-MM-dd}",
                    ReferenceNo = null,
                    IsVoided = false
                };

                await _unitOfWork.FolioTransactions.AddAsync(transactionEntity);

                await _unitOfWork.GuestFolios.GetQueryable()
                    .Where(f => f.Id == folio.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(f => f.TotalCharges, f => f.TotalCharges + chargeAmount)
                        .SetProperty(f => f.Balance, f => f.Balance + chargeAmount));
                
                processedRooms++;
            }

            _logger.LogInformation("Posted room charges for {ProcessedRooms} reservations on BusinessDate={Date}.",
                processedRooms, currentBusinessDate);

            return processedRooms;
        }

        private async Task<int> HandleNoShowsAsync(DateTime currentBusinessDate)
        {
            var noShowsMarked = await _unitOfWork.Reservations
                .GetQueryable()
                .Where(r =>
                    (r.Status == ReservationStatus.Confirmed ||
                     r.Status == ReservationStatus.Pending) &&
                    r.CheckInDate.Date <= currentBusinessDate.Date)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.NoShow));

            _logger.LogInformation("Marked {NoShows} reservations as NoShow for BusinessDate={Date}.",
                noShowsMarked, currentBusinessDate);

            return noShowsMarked;
        }

        private async Task<DateTime> RollBusinessDateAsync(BusinessDay currentBusinessDay, string userId, DateTime currentBusinessDate)
        {
            currentBusinessDay.Status = BusinessDayStatus.Closed;
            currentBusinessDay.EndedAt = DateTime.UtcNow;
            currentBusinessDay.ClosedById = userId;

            _unitOfWork.BusinessDays.Update(currentBusinessDay);

            var newBusinessDay = new BusinessDay
            {
                Date = currentBusinessDate.AddDays(1),
                Status = BusinessDayStatus.Open,
                StartedAt = DateTime.UtcNow,
                EndedAt = null,
                ClosedById = null
            };

            await _unitOfWork.BusinessDays.AddAsync(newBusinessDay);

            return newBusinessDay.Date;
        }

        private class NightAuditValidationException : Exception
        {
            public NightAuditValidationException(string message) : base(message)
            {
            }
        }
    }
}

