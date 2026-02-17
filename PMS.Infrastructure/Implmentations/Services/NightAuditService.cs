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
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<NightAuditService> _logger;

        public NightAuditService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext dbContext,
            ILogger<NightAuditService> logger)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
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

            // Always work with a pure date component for the business day.
            var currentBusinessDate = (await _unitOfWork.GetCurrentBusinessDateAsync()).Date;

            _logger.LogInformation("Starting night audit for BusinessDate={Date} by User={UserId}, Force={Force}",
                currentBusinessDate, userId, force);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var response = new AuditResponseDto
                {
                    NewBusinessDate = currentBusinessDate,
                    Message = string.Empty,
                    NoShowsMarked = 0,
                    ProcessedRooms = 0
                };

                // Load the current open BusinessDay (must exist for a valid audit).
                var currentBusinessDay = await _unitOfWork.BusinessDays
                    .GetQueryable()
                    .FirstOrDefaultAsync(b => b.Status == BusinessDayStatus.Open);

                if (currentBusinessDay == null)
                {
                    _logger.LogWarning("Night audit aborted: no open BusinessDay found.");
                    await transaction.RollbackAsync();
                    return new ApiResponse<AuditResponseDto>("No open BusinessDay found to run night audit.");
                }

                // ===========================
                // Step A: Validation
                // ===========================
                if (!force)
                {
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
                        var message =
                            $"Night audit blocked: {pendingOrConfirmedArrivals} reservations with status Pending/Confirmed still have Check-In = business date {currentBusinessDate:yyyy-MM-dd}.";
                        _logger.LogWarning(message);
                        await transaction.RollbackAsync();
                        return new ApiResponse<AuditResponseDto>(message);
                    }

                    _logger.LogInformation("Night audit validation passed: no pending/confirmed arrivals for BusinessDate={Date}.",
                        currentBusinessDate);
                }
                else
                {
                    _logger.LogInformation("Night audit running in FORCE mode. Validation step skipped.");
                }

                // ===========================
                // Step B: Post Room Charges
                // ===========================

                var checkedInReservations = await _unitOfWork.Reservations
                    .GetQueryable()
                    .Where(r =>
                        r.Status == ReservationStatus.CheckIn &&
                        r.CheckOutDate.Date > currentBusinessDate)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} checked-in reservations eligible for room charges on BusinessDate={Date}.",
                    checkedInReservations.Count, currentBusinessDate);

                if (checkedInReservations.Any())
                {
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

                        folio.TotalCharges += chargeAmount;
                        folio.Balance = folio.TotalCharges - folio.TotalPayments;

                        _unitOfWork.GuestFolios.Update(folio);

                        response.ProcessedRooms++;
                    }

                    _logger.LogInformation("Posted room charges for {ProcessedRooms} reservations on BusinessDate={Date}.",
                        response.ProcessedRooms, currentBusinessDate);
                }

                // ===========================
                // Step C: Handle No-Shows
                // ===========================

                var noShowCandidates = await _unitOfWork.Reservations
                    .GetQueryable()
                    .Where(r =>
                        (r.Status == ReservationStatus.Confirmed ||
                         r.Status == ReservationStatus.Pending) &&
                        r.CheckInDate.Date <= currentBusinessDate.Date)
                    .ToListAsync();

                foreach (var reservation in noShowCandidates)
                {
                    reservation.Status = ReservationStatus.NoShow;
                    _unitOfWork.Reservations.Update(reservation);
                }

                response.NoShowsMarked = noShowCandidates.Count;

                _logger.LogInformation("Marked {NoShows} reservations as NoShow for BusinessDate={Date}.",
                    response.NoShowsMarked, currentBusinessDate);

                // ===========================
                // Step D: Roll Business Date
                // ===========================

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

                response.NewBusinessDate = newBusinessDay.Date;
                response.Message = $"Night audit completed successfully. New business date is {response.NewBusinessDate:yyyy-MM-dd}.";

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Night audit completed successfully for BusinessDate={OldDate}. NewBusinessDate={NewDate}, ProcessedRooms={ProcessedRooms}, NoShows={NoShows}.",
                    currentBusinessDate, response.NewBusinessDate, response.ProcessedRooms, response.NoShowsMarked);

                return new ApiResponse<AuditResponseDto>(response, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Night audit failed for BusinessDate={Date}. Rolling back transaction.", currentBusinessDate);
                await transaction.RollbackAsync();
                return new ApiResponse<AuditResponseDto>($"Night audit failed: {ex.Message}");
            }
        }
    }
}

