using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IAPInvoiceService
    {
        Task<ApiResponse<APInvoiceDto>> CreateInvoiceAsync(CreateAPInvoiceDto dto);
        Task<ApiResponse<APInvoiceDto>> GetInvoiceByIdAsync(int invoiceId);
        Task<PagedResult<APInvoiceDto>> GetAllInvoicesAsync(int pageNumber, int pageSize, int? vendorId);
        Task<ApiResponse<APInvoiceDto>> ApproveInvoiceAsync(int invoiceId);
        Task<ApiResponse<bool>> VoidInvoiceAsync(int invoiceId);
    }
}
