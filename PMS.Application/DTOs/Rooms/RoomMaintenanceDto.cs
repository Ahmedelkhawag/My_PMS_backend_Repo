using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Rooms
{
    public class RoomMaintenanceDto
    {
        [Required(ErrorMessage = "Maintenance reason is required.")]
        public string Reason { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Remarks { get; set; }
    }
}
