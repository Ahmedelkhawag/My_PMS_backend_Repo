namespace PMS.Application.DTOs.Dashboard
{
    public record DashboardSummaryDto
    {
        public RoomStatsDto RoomStats { get; init; } = new();
        public ReservationStatsDto ReservationStats { get; init; } = new();
        public GuestStatsDto GuestStats { get; init; } = new();
        public decimal CurrentReceivables { get; init; }
    }

    public record RoomStatsDto
    {
        public int TotalRooms { get; init; }
        public int AvailableRooms { get; init; }
        public int OccupiedRooms { get; init; }
        public int DirtyRooms { get; init; }
        public int OutOfService { get; init; }
        public decimal OccupancyPercentage { get; init; }
    }

    public record ReservationStatsDto
    {
        public int TotalReservations { get; init; }
        public int CreatedToday { get; init; }
        public int ArrivalsToday { get; init; }
        public int DeparturesToday { get; init; }
        public int ActiveInHouse { get; init; }
        public int PendingConfirmations { get; init; }
    }

    public record GuestStatsDto
    {
        public int TotalGuests { get; init; }
        public int NewGuestsThisMonth { get; init; }
    }
}
