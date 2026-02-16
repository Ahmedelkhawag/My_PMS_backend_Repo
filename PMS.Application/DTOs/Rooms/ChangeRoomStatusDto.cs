using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Rooms
{
    public class ChangeRoomStatusDto
    {
        public RoomStatusType StatusType { get; set; }
        public int StatusId { get; set; }
        public string? Notes { get; set; }
    }
}

