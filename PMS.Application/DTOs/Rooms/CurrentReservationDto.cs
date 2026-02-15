namespace PMS.Application.DTOs.Rooms
{
    /// <summary>
    /// Current in-house reservation for a room (when FO status is Occupied).
    /// </summary>
    public class CurrentReservationDto
    {
        public int Id { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string ArrivalDate { get; set; } = string.Empty;  // ISO date
        public string DepartureDate { get; set; } = string.Empty; // ISO date
        /// <summary>Balance due. Uses GrandTotal until payment module exists.</summary>
        public decimal Balance { get; set; }
    }
}
