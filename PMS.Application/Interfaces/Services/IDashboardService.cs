using System.Threading.Tasks;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;

namespace PMS.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<ResponseObjectDto<DashboardSummaryDto>> GetDashboardSummaryAsync();
    }
}

