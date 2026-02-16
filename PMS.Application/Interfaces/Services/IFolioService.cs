using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Folios;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IFolioService
    {
        Task<ResponseObjectDto<GuestFolioSummaryDto>> CreateFolioForReservationAsync(int reservationId);

        Task<ResponseObjectDto<FolioTransactionDto>> AddTransactionAsync(CreateTransactionDto dto);

        Task<ResponseObjectDto<FolioTransactionDto>> VoidTransactionAsync(int transactionId);
    }
}

