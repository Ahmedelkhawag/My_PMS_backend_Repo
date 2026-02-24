using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Rooms
{
    public record ChangeRoomStatusDto
    {
        public RoomStatusType StatusType { get; init; }
        public int StatusId { get; init; }
        public string? Notes { get; init; }
    }
}
