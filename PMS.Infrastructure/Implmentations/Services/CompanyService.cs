using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Companies;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseObjectDto<CompanyProfileDto>> CreateCompanyAsync(CreateCompanyProfileDto dto)
        {
            var company = new CompanyProfile
            {
                Name = dto.Name,
                TaxNumber = dto.TaxNumber,
                ContactPerson = dto.ContactPerson,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                ContractRateId = dto.ContractRateId
            };

            await _unitOfWork.CompanyProfiles.AddAsync(company);
            await _unitOfWork.CompleteAsync();

            var result = new CompanyProfileDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxNumber = company.TaxNumber,
                ContactPerson = company.ContactPerson,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                Address = company.Address,
                ContractRateId = company.ContractRateId,
                CreatedBy = company.CreatedBy,
                CreatedAt = company.CreatedAt,
                UpdatedBy = company.LastModifiedBy,
                UpdatedAt = company.LastModifiedAt
            };

            return new ResponseObjectDto<CompanyProfileDto>
            {
                IsSuccess = true,
                Message = "Company created successfully",
                StatusCode = 201,
                Data = result
            };
        }

        public async Task<ResponseObjectDto<PagedResult<CompanyProfileDto>>> GetAllCompaniesAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CompanyProfiles.GetQueryable()
                .Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c => c.Name.Contains(term) || (c.TaxNumber != null && c.TaxNumber.Contains(term)));
            }

            var totalCount = await query.CountAsync();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CompanyProfileDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    TaxNumber = c.TaxNumber,
                    ContactPerson = c.ContactPerson,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address,
                    ContractRateId = c.ContractRateId,
                    CreatedBy = c.CreatedBy,
                    CreatedAt = c.CreatedAt,
                    UpdatedBy = c.LastModifiedBy,
                    UpdatedAt = c.LastModifiedAt
                })
                .ToListAsync();

            var paged = new PagedResult<CompanyProfileDto>(items, totalCount, pageNumber, pageSize);

            return new ResponseObjectDto<PagedResult<CompanyProfileDto>>
            {
                IsSuccess = true,
                Message = "Companies retrieved successfully",
                StatusCode = 200,
                Data = paged
            };
        }

        public async Task<ResponseObjectDto<CompanyProfileDto>> GetCompanyByIdAsync(int id)
        {
            var company = await _unitOfWork.CompanyProfiles.GetByIdAsync(id);
            if (company == null || company.IsDeleted)
            {
                return new ResponseObjectDto<CompanyProfileDto>
                {
                    IsSuccess = false,
                    Message = "Company not found",
                    StatusCode = 404
                };
            }

            var dto = new CompanyProfileDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxNumber = company.TaxNumber,
                ContactPerson = company.ContactPerson,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                Address = company.Address,
                ContractRateId = company.ContractRateId,
                CreatedBy = company.CreatedBy,
                CreatedAt = company.CreatedAt,
                UpdatedBy = company.LastModifiedBy,
                UpdatedAt = company.LastModifiedAt
            };

            return new ResponseObjectDto<CompanyProfileDto>
            {
                IsSuccess = true,
                Message = "Company retrieved successfully",
                StatusCode = 200,
                Data = dto
            };
        }

        public async Task<ResponseObjectDto<bool>> UpdateCompanyAsync(UpdateCompanyProfileDto dto)
        {
            var company = await _unitOfWork.CompanyProfiles.GetByIdAsync(dto.Id);
            if (company == null || company.IsDeleted)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    Message = "Company not found",
                    StatusCode = 404
                };
            }

            company.Name = dto.Name;
            company.TaxNumber = dto.TaxNumber;
            company.ContactPerson = dto.ContactPerson;
            company.PhoneNumber = dto.PhoneNumber;
            company.Email = dto.Email;
            company.Address = dto.Address;
            company.ContractRateId = dto.ContractRateId;

            _unitOfWork.CompanyProfiles.Update(company);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                Message = "Company updated successfully",
                StatusCode = 200,
                Data = true
            };
        }

        public async Task<ResponseObjectDto<bool>> DeleteCompanyAsync(int id)
        {
            var company = await _unitOfWork.CompanyProfiles.GetByIdAsync(id);
            if (company == null || company.IsDeleted)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    Message = "Company not found",
                    StatusCode = 404
                };
            }

            _unitOfWork.CompanyProfiles.Delete(company);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                Message = "Company deleted successfully",
                StatusCode = 200,
                Data = true
            };
        }
    }
}
