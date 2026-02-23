using PMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Reservations
{
    /// <summary>
    /// Body for PUT /api/reservations/{id}/status. Reservation id comes from route.
    /// </summary>
    public class SetReservationStatusDto
    {
        /// <summary>
        /// حالة الحجز (1 = Pending, 2 = Confirmed, 3 = CheckIn, 4 = CheckOut, 5 = Cancelled, 6 = NoShow).
        /// </summary>
        [Required]
        public ReservationStatus NewStatus { get; set; }

        public int? RoomId { get; set; }

        public string? Note { get; set; }

        public decimal? FeeAmount { get; set; }

        public string? FeeReason { get; set; }
    }
}
