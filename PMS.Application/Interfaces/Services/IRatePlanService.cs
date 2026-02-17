using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Configuration;

namespace PMS.Application.Interfaces.Services
{
    public interface IRatePlanService
    {
        Task<ResponseObjectDto<List<RatePlanDto>>> GetAllAsync(bool? isPublicOnly = null);

        Task<ResponseObjectDto<RatePlanDto>> GetByIdAsync(int id);

        Task<ResponseObjectDto<RatePlanDto>> CreateAsync(CreateRatePlanDto dto);

		Task<ResponseObjectDto<RatePlanDto>> UpdateAsync(int id, UpdateRatePlanDto dto);

		Task<ResponseObjectDto<bool>> DeleteAsync(int id);

		Task<ResponseObjectDto<bool>> RestoreAsync(int id);
    }
}

