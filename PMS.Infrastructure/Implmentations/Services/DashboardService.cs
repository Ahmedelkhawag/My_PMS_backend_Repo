using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Enums;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        private const int ROOM_STATUS_CLEAN = 1;
        private const int ROOM_STATUS_DIRTY = 2;
        private const int ROOM_STATUS_MAINTENANCE = 3;
        private const int ROOM_STATUS_OUT_OF_ORDER = 4;
        private const int ROOM_STATUS_OCCUPIED = 5;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseObjectDto<DashboardSummaryDto>> GetDashboardSummaryAsync()
        {
            var response = new ResponseObjectDto<DashboardSummaryDto>();

            var today = DateTime.UtcNow.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var roomsQuery = _unitOfWork.Rooms
                .GetQueryable()
                .Where(r => !r.IsDeleted && r.IsActive);

            var reservationsQuery = _unitOfWork.Reservations
                .GetQueryable()
                .Where(r => !r.IsDeleted);

            var guestsQuery = _unitOfWork.Guests
                .GetQueryable()
                .Where(g => !g.IsDeleted);

            // NOTE:
            // EF Core لا يسمح بتشغيل أكثر من استعلام Async في نفس الوقت
            // على نفس الـ DbContext. لذلك ننفذ كل CountAsync بالتسلسل.

            var totalRooms = await roomsQuery.CountAsync();
            var availableRooms = await roomsQuery.CountAsync(r => r.RoomStatusId == ROOM_STATUS_CLEAN);
            var occupiedRooms = await roomsQuery.CountAsync(r => r.RoomStatusId == ROOM_STATUS_OCCUPIED);
            var dirtyRooms = await roomsQuery.CountAsync(r => r.RoomStatusId == ROOM_STATUS_DIRTY);
            var outOfServiceRooms = await roomsQuery.CountAsync(r =>
                r.RoomStatusId == ROOM_STATUS_MAINTENANCE || r.RoomStatusId == ROOM_STATUS_OUT_OF_ORDER);

            var createdToday = await reservationsQuery.CountAsync(r => r.CreatedAt.Date == today);
            var arrivalsToday = await reservationsQuery.CountAsync(r =>
                r.CheckInDate.Date == today &&
                r.Status == ReservationStatus.Confirmed);
            var departuresToday = await reservationsQuery.CountAsync(r =>
                r.CheckOutDate.Date == today &&
                r.Status == ReservationStatus.CheckIn);
            var activeInHouse = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.CheckIn);
            var pendingConfirmations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Pending);

            var totalGuests = await guestsQuery.CountAsync();
            var newGuestsThisMonth = await guestsQuery.CountAsync(g =>
                g.CreatedAt >= firstDayOfMonth && g.CreatedAt < firstDayOfNextMonth);

            decimal occupancyPercentage = 0;
            if (totalRooms > 0 && occupiedRooms > 0)
            {
                occupancyPercentage = (decimal)occupiedRooms / totalRooms * 100m;
            }

            var summary = new DashboardSummaryDto
            {
                RoomStats = new RoomStatsDto
                {
                    TotalRooms = totalRooms,
                    AvailableRooms = availableRooms,
                    OccupiedRooms = occupiedRooms,
                    DirtyRooms = dirtyRooms,
                    OutOfService = outOfServiceRooms,
                    OccupancyPercentage = occupancyPercentage
                },
                ReservationStats = new ReservationStatsDto
                {
                    CreatedToday = createdToday,
                    ArrivalsToday = arrivalsToday,
                    DeparturesToday = departuresToday,
                    ActiveInHouse = activeInHouse,
                    PendingConfirmations = pendingConfirmations
                },
                GuestStats = new GuestStatsDto
                {
                    TotalGuests = totalGuests,
                    NewGuestsThisMonth = newGuestsThisMonth
                }
            };

            response.IsSuccess = true;
            response.StatusCode = 200;
            response.Message = "Dashboard summary retrieved successfully";
            response.Data = summary;

            return response;
        }
    }
}

