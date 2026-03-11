using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IARPaymentService
    {
        Task<ApiResponse<int>> ProcessPaymentAsync(ProcessPaymentDto dto);
        Task<ApiResponse<bool>> AllocatePaymentAsync(int paymentId, List<AllocationRequest> requests);
    }
}
