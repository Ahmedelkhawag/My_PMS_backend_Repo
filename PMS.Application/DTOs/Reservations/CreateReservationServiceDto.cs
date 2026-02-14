using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Reservations
{
    public class CreateReservationServiceDto
    {
        [Required(ErrorMessage = "Service ID is required")]
        public int ExtraServiceId { get; set; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;
    }
}
