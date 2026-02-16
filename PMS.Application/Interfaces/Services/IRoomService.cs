using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IRoomService
    {
        Task<ResponseObjectDto<PagedResult<RoomDto>>> GetAllRoomsAsync(int? floor, int? roomTypeId, string? status, int pageNumber, int pageSize);
        Task<ResponseObjectDto<RoomDto>> GetRoomByIdAsync(int id);

        Task<ResponseObjectDto<RoomDto>> CreateRoomAsync(CreateRoomDto dto);

        Task<ResponseObjectDto<RoomDto>> UpdateRoomAsync(int id, UpdateRoomDto dto);
        Task<ResponseObjectDto<bool>> DeleteRoomAsync(int id);

        Task<ResponseObjectDto<bool>> ChangeRoomStatusAsync(int roomId, ChangeRoomStatusDto dto);
        Task<ResponseObjectDto<bool>> RestoreRoomAsync(int id);
        Task<ResponseObjectDto<RoomStatsDto>> GetRoomStatsAsync();
        Task<ResponseObjectDto<bool>> StartMaintenanceAsync(int roomId, RoomMaintenanceDto dto);
        Task<ResponseObjectDto<bool>> FinishMaintenanceAsync(int roomId);
    }
}
