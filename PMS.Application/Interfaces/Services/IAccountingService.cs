using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.BackOffice;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IAccountingService
    {
        Task<ApiResponse<int>> CreateAccountAsync(CreateAccountDto dto);
        Task<ApiResponse<List<AccountTreeDto>>> GetAccountsTreeAsync();
        
        Task<ApiResponse<int>> CreateCostCenterAsync(CreateCostCenterDto dto);
        Task<ApiResponse<List<CostCenterDto>>> GetCostCentersTreeAsync();

        Task<ApiResponse<bool>> PostTransactionToGLAsync(int folioTransactionId);
        Task<ApiResponse<bool>> ReverseTransactionInGLAsync(int folioTransactionId);
        Task<ApiResponse<int>> PostAPInvoiceToGLAsync(int invoiceId);
        Task<ApiResponse<bool>> ReverseJournalEntryAsync(int journalEntryId, string referenceCode, string description);
        Task<ApiResponse<bool>> PostARPaymentToGLAsync(int arPaymentId);
        Task<ApiResponse<bool>> PostARAdjustmentToGLAsync(int arAdjustmentId);
        Task<ApiResponse<int>> PostAPPaymentToGLAsync(int paymentId, int creditAccountId);
        Task<ApiResponse<bool>> CreateManualJournalEntryAsync(CreateJournalEntryDto dto);
        Task<ApiResponse<TrialBalanceReportDto>> GetTrialBalanceAsync();
        Task<ApiResponse<PnLReportDto>> GetPnLReportAsync(DateTime startDate, DateTime endDate, int? costCenterId = null);
        Task<ApiResponse<BalanceSheetDto>> GetBalanceSheetAsync(DateTime asOfDate);
        Task<ApiResponse<AccountStatementHeaderDto>> GetAccountStatementAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<IEnumerable<int>>> GetUnpostedTransactionsAsync();
        Task<ApiResponse<bool>> ApproveJournalEntryAsync(int id, string userId);
        Task<ApiResponse<bool>> RejectJournalEntryAsync(int id, string userId, string reason);
    }
}
