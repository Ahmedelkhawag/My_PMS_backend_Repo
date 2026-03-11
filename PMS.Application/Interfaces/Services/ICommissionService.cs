using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface ICommissionService
    {
        /// <summary>
        /// Calculates or recalculates the TA commission for the given reservation.
        /// Only proceeds if the linked CompanyProfile has a CommissionRate > 0.
        /// Creates a new TACommissionRecord (Draft) or updates an existing Draft record.
        /// </summary>
        Task<ApiResponse<bool>> CalculateForReservationAsync(int reservationId);

        /// <summary>Returns all TACommissionRecords with status Draft.</summary>
        Task<ApiResponse<IEnumerable<TACommissionRecordDto>>> GetPendingCommissionsAsync();

        /// <summary>
        /// Approves the commission record, posts a GL Journal Entry (Debit Commission Expense / Credit Commissions Payable),
        /// and links the JournalEntryId back to the record.
        /// </summary>
        Task<ApiResponse<bool>> ApproveCommissionAsync(int recordId);
    }
}
