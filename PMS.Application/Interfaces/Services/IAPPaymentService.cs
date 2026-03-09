using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IAPPaymentService
    {
        Task<ApiResponse<APPaymentDto>> CreatePaymentAsync(CreateAPPaymentDto dto);
        Task<ApiResponse<bool>> VoidPaymentAsync(int paymentId);
        Task<ApiResponse<APPaymentDto>> GetPaymentByIdAsync(int paymentId);
        Task<PagedResult<APPaymentDto>> GetAllPaymentsAsync(int pageNumber, int pageSize, int? vendorId);
    }
}

