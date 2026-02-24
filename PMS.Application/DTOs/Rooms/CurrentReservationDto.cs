namespace PMS.Application.DTOs.Rooms
{
    /// <summary>
    /// Current in-house reservation for a room (when FO status is Occupied).
    /// </summary>
    public record CurrentReservationDto
    {
        public int Id { get; init; }
        public string GuestName { get; init; } = string.Empty;
        public string ArrivalDate { get; init; } = string.Empty;
        public string DepartureDate { get; init; } = string.Empty;
        /// <summary>Balance due. Uses GrandTotal until payment module exists.</summary>
        public decimal Balance { get; init; }
    }
}
