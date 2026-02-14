using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Companies;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using System;
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
            var nameTrim = dto.Name?.Trim() ?? string.Empty;
            var emailTrim = dto.Email?.Trim() ?? string.Empty;
            var phoneTrim = dto.PhoneNumber?.Trim() ?? string.Empty;
            var taxTrim = string.IsNullOrWhiteSpace(dto.TaxNumber) ? null : dto.TaxNumber.Trim();

            var existing = await _unitOfWork.CompanyProfiles.GetQueryable()
                .Where(c => !c.IsDeleted &&
                    (c.Name == nameTrim ||
                     (taxTrim != null && c.TaxNumber == taxTrim) ||
                     c.Email == emailTrim ||
                     c.PhoneNumber == phoneTrim))
                .FirstOrDefaultAsync();
            if (existing != null)
            {
                return new ResponseObjectDto<CompanyProfileDto>
                {
                    IsSuccess = false,
                    StatusCode = 409,
                    Message = GetDuplicateFieldMessage(existing, nameTrim, taxTrim, emailTrim, phoneTrim)
                };
            }

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

        public async Task<ResponseObjectDto<bool>> UpdateCompanyAsync(int id, UpdateCompanyProfileDto dto)
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

            var nameProposed = dto.Name?.Trim() ?? company.Name;
            var emailProposed = dto.Email?.Trim() ?? company.Email;
            var phoneProposed = dto.PhoneNumber?.Trim() ?? company.PhoneNumber;
            var taxProposed = !string.IsNullOrWhiteSpace(dto.TaxNumber) ? dto.TaxNumber.Trim() : company.TaxNumber;

            var existing = await _unitOfWork.CompanyProfiles.GetQueryable()
                .Where(c => !c.IsDeleted && c.Id != id &&
                    (c.Name == nameProposed ||
                     (taxProposed != null && c.TaxNumber == taxProposed) ||
                     c.Email == emailProposed ||
                     c.PhoneNumber == phoneProposed))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 409,
                    Message = GetDuplicateFieldMessage(existing, nameProposed, taxProposed, emailProposed, phoneProposed)
                };
            }

            if (dto.Name != null) company.Name = dto.Name.Trim();
            if (dto.TaxNumber != null) company.TaxNumber = dto.TaxNumber;
            if (dto.ContactPerson != null) company.ContactPerson = dto.ContactPerson.Trim();
            if (dto.PhoneNumber != null) company.PhoneNumber = dto.PhoneNumber.Trim();
            if (dto.Email != null) company.Email = dto.Email.Trim();
            if (dto.Address != null) company.Address = dto.Address;
            if (dto.ContractRateId.HasValue) company.ContractRateId = dto.ContractRateId;

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

        private static string GetDuplicateFieldMessage(CompanyProfile existing, string name, string? taxNumber, string email, string phoneNumber)
        {
            var nameTrim = name?.Trim() ?? string.Empty;
            var emailTrim = email?.Trim() ?? string.Empty;
            var phoneTrim = phoneNumber?.Trim() ?? string.Empty;

            if (existing.Name.Equals(nameTrim, StringComparison.OrdinalIgnoreCase))
                return "Company name already exists";
            if (!string.IsNullOrWhiteSpace(taxNumber) && existing.TaxNumber != null && existing.TaxNumber.Equals(taxNumber.Trim(), StringComparison.Ordinal))
                return "Tax Number already exists";
            if (existing.Email.Equals(emailTrim, StringComparison.OrdinalIgnoreCase))
                return "Email address is already registered";
            if (existing.PhoneNumber?.Trim() == phoneTrim)
                return "Phone number is already registered";

            return "A company with the same details already exists";
        }
    }
}
