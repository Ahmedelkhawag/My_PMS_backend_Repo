using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface IVendorService
    {
        Task<PagedResult<VendorDto>> GetAllVendorsAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ApiResponse<VendorDto>> GetVendorByIdAsync(int id);
        Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto dto);
        Task<ApiResponse<VendorDto>> UpdateVendorAsync(int id, UpdateVendorDto dto);
        Task<ApiResponse<bool>> DeleteVendorAsync(int id);
    }
}
