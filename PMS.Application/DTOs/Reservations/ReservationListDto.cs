namespace PMS.Application.DTOs.Reservations
{
    public record ReservationListDto
    {
        public int Id { get; init; }
        public string ReservationNumber { get; init; } = string.Empty;
        public string GuestName { get; init; } = string.Empty;
        public string GuestPhone { get; init; } = string.Empty;
        public string? RoomNumber { get; init; }
        public string RoomTypeName { get; init; } = string.Empty;
        public string? RatePlanName { get; init; }
        public string CheckInDate { get; init; } = string.Empty;
        public string CheckOutDate { get; init; } = string.Empty;
        public int Nights { get; init; }
        public decimal GrandTotal { get; init; }
        public string Status { get; init; } = string.Empty;
        public string StatusColor { get; init; } = string.Empty;
        public string? Notes { get; init; }
    }
}
