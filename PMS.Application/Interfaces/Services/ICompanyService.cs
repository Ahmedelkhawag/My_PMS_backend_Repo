using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Companies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<ResponseObjectDto<CompanyProfileDto>> CreateCompanyAsync(CreateCompanyProfileDto dto);
        Task<ResponseObjectDto<List<CompanyProfileDto>>> GetAllCompaniesAsync();
    }
}
