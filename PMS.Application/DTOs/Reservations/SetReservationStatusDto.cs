using PMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Reservations
{
    /// <summary>
    /// Body for PUT /api/reservations/{id}/status. Reservation id comes from route.
    /// </summary>
    public class SetReservationStatusDto
    {
        [Required]
        public ReservationStatus NewStatus { get; set; }

        public int? RoomId { get; set; }

        public string? Note { get; set; }
    }
}
