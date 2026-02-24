using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Rooms
{
    /// <summary>
    /// Partial update: send only the fields you want to change.
    /// </summary>
    public record UpdateRoomDto
    {
        public string? RoomNumber { get; init; }

        [Range(1, 100, ErrorMessage = "Floor number is invalid.")]
        public int? FloorNumber { get; init; }

        public int? RoomTypeId { get; init; }

        public string? Notes { get; init; }

        public string? Status { get; init; }
    }
}
