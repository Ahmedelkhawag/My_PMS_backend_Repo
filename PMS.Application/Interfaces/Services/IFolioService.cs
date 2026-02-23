using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Folios;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IFolioService
    {
        Task<ResponseObjectDto<GuestFolioSummaryDto>> GetFolioSummaryAsync(int reservationId);

        Task<ResponseObjectDto<FolioDetailsDto>> GetFolioDetailsAsync(int reservationId);

        Task<ResponseObjectDto<bool>> CloseFolioAsync(int reservationId);

        Task<ResponseObjectDto<FolioTransactionDto>> AddTransactionAsync(CreateTransactionDto dto);

        Task<ResponseObjectDto<FolioTransactionDto>> AddTransactionWithoutCommitAsync(CreateTransactionDto dto);

        Task<ResponseObjectDto<FolioTransactionDto>> VoidTransactionAsync(int transactionId);

        Task<ResponseObjectDto<bool>> PostPaymentWithDiscountAsync(PostPaymentWithDiscountDto dto);

        Task<ResponseObjectDto<FolioTransactionDto>> RefundTransactionAsync(int originalTransactionId, decimal refundAmount, string reason, string userId);

        Task<ResponseObjectDto<bool>> TransferTransactionAsync(int transactionId, int targetReservationId, string userId, string reason);
    }
}

