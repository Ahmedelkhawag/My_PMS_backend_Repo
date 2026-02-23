using PMS.Application.DTOs;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Shifts;

namespace PMS.Application.Interfaces.Services
{
	public interface IShiftService
	{
        Task<ResponseObjectDto<ShiftDto>> OpenShiftAsync(string userId, OpenShiftDto dto);
        Task<ResponseObjectDto<ShiftDto>> CloseShiftAsync(string userId, CloseShiftDto dto);
        Task<ResponseObjectDto<ShiftReportDto>> GetCurrentShiftStatusAsync(string userId);
        Task<ResponseObjectDto<IEnumerable<ShiftDto>>> GetShiftHistoryAsync(UserFilterDto filter);
    }
}

