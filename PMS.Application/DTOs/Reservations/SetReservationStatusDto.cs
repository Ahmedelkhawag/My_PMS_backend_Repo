using PMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Reservations
{
    /// <summary>Body for PUT /api/reservations/{id}/status. Reservation id comes from route.</summary>
    public record SetReservationStatusDto
    {
        /// <summary>New status (1=Pending, 2=Confirmed, 3=CheckIn, 4=CheckOut, 5=Cancelled, 6=NoShow).</summary>
        [Required]
        public ReservationStatus NewStatus { get; init; }

        public int? RoomId { get; init; }
        public string? Note { get; init; }
        public decimal? FeeAmount { get; init; }
        public string? FeeReason { get; init; }
    }
}
