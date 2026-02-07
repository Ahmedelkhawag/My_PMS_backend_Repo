using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int? floor, int? roomTypeId, string? status);
        Task<ResponseObjectDto<RoomDto>> CreateRoomAsync(CreateRoomDto dto);

        Task<ResponseObjectDto<RoomDto>> UpdateRoomAsync(UpdateRoomDto dto);
        Task<ResponseObjectDto<bool>> DeleteRoomAsync(int id);
    }
}
