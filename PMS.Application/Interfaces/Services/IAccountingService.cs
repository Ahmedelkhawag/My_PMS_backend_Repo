using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.BackOffice;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IAccountingService
    {
        Task<ApiResponse<bool>> PostTransactionToGLAsync(int folioTransactionId);
        Task<ApiResponse<bool>> ReverseTransactionInGLAsync(int folioTransactionId);
        Task<ApiResponse<int>> PostAPInvoiceToGLAsync(int invoiceId);
        Task<ApiResponse<bool>> ReverseJournalEntryAsync(int journalEntryId, string referenceCode, string description);
        Task<ApiResponse<bool>> PostARPaymentToGLAsync(int arPaymentId);
        Task<ApiResponse<bool>> PostARAdjustmentToGLAsync(int arAdjustmentId);
        Task<ApiResponse<int>> PostAPPaymentToGLAsync(int paymentId, int creditAccountId);
        Task<ApiResponse<bool>> CreateManualJournalEntryAsync(CreateJournalEntryDto dto);
        Task<ApiResponse<TrialBalanceReportDto>> GetTrialBalanceAsync();
        Task<ApiResponse<AccountStatementHeaderDto>> GetAccountStatementAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<IEnumerable<int>>> GetUnpostedTransactionsAsync();
    }
}

