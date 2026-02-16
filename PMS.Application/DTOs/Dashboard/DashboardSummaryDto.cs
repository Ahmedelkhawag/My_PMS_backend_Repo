using System;

namespace PMS.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public RoomStatsDto RoomStats { get; set; } = new RoomStatsDto();
        public ReservationStatsDto ReservationStats { get; set; } = new ReservationStatsDto();
        public GuestStatsDto GuestStats { get; set; } = new GuestStatsDto();
        public decimal CurrentReceivables { get; set; }
    }

    public class RoomStatsDto
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int DirtyRooms { get; set; }
        public int OutOfService { get; set; }
        public decimal OccupancyPercentage { get; set; }
    }

    public class ReservationStatsDto
    {
        public int TotalReservations { get; set; }
        public int CreatedToday { get; set; }
        public int ArrivalsToday { get; set; }
        public int DeparturesToday { get; set; }
        public int ActiveInHouse { get; set; }
        public int PendingConfirmations { get; set; }
    }

    public class GuestStatsDto
    {
        public int TotalGuests { get; set; }
        public int NewGuestsThisMonth { get; set; }
    }
}

