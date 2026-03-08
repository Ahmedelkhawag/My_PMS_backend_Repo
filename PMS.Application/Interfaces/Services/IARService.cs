using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IARService
    {
        Task<ApiResponse<bool>> TransferFolioToARAsync(TransferFolioDto dto);
        Task<ApiResponse<bool>> ReceiveARPaymentAsync(ReceiveARPaymentDto dto);
        Task<ApiResponse<CompanyStatementReportDto>> GetCompanyStatementAsync(int companyId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<ARAgingReportDto>> GetARAgingReportAsync();
        Task<ApiResponse<bool>> CreateAdjustmentAsync(CreateARAdjustmentDto dto);
        Task<ApiResponse<CompanySOAPdfResultDto>> GenerateCompanySOAInPdfAsync(int companyId, DateTime startDate, DateTime endDate);
    }
}

