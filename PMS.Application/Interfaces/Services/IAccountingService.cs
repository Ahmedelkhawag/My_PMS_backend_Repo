using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.BackOffice;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IAccountingService
    {
        Task<ApiResponse<bool>> PostTransactionToGLAsync(int folioTransactionId);
        Task<ApiResponse<bool>> CreateManualJournalEntryAsync(CreateJournalEntryDto dto);
        Task<ApiResponse<TrialBalanceReportDto>> GetTrialBalanceAsync();
        Task<ApiResponse<AccountStatementHeaderDto>> GetAccountStatementAsync(int accountId, DateTime startDate, DateTime endDate);
    }
}

