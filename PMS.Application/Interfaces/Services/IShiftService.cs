using PMS.Application.DTOs;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Shifts;

namespace PMS.Application.Interfaces.Services
{
	public interface IShiftService
	{
		Task<ApiResponse<ShiftDto>> OpenShiftAsync(string userId, OpenShiftDto dto);
		Task<ApiResponse<ShiftDto>> CloseShiftAsync(string userId, CloseShiftDto dto);
		Task<ApiResponse<ShiftReportDto>> GetCurrentShiftStatusAsync(string userId);
		Task<ApiResponse<IEnumerable<ShiftDto>>> GetShiftHistoryAsync(UserFilterDto filter);
	}
}

