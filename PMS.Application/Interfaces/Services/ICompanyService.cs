using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Companies;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<ResponseObjectDto<CompanyProfileDto>> CreateCompanyAsync(CreateCompanyProfileDto dto);
        Task<ResponseObjectDto<PagedResult<CompanyProfileDto>>> GetAllCompaniesAsync(string? search, int pageNumber, int pageSize);
        Task<ResponseObjectDto<CompanyProfileDto>> GetCompanyByIdAsync(int id);
        Task<ResponseObjectDto<bool>> UpdateCompanyAsync(UpdateCompanyProfileDto dto);
        Task<ResponseObjectDto<bool>> DeleteCompanyAsync(int id);
    }
}
