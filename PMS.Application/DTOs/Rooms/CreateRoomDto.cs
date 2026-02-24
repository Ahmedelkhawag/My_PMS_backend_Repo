using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Rooms
{
    public record CreateRoomDto
    {
        [Required(ErrorMessage = "Room number is required.")]
        public string RoomNumber { get; init; } = string.Empty;

        [Required(ErrorMessage = "Floor number is required.")]
        [Range(1, 100, ErrorMessage = "Floor number must be valid.")]
        public int FloorNumber { get; init; }

        [Required(ErrorMessage = "Room type is required.")]
        public int RoomTypeId { get; init; }

        public string? Notes { get; init; }
    }
}
