using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Rooms
{
    public record RoomMaintenanceDto
    {
        [Required(ErrorMessage = "Maintenance reason is required.")]
        public string Reason { get; init; } = string.Empty;

        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? Remarks { get; init; }
    }
}
