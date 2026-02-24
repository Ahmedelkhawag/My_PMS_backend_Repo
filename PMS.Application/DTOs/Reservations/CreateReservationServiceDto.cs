using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Reservations
{
    public record CreateReservationServiceDto
    {
        [Required(ErrorMessage = "Service ID is required")]
        public int ExtraServiceId { get; init; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; init; } = 1;
    }
}
