using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Companies;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using System.Collections.Generic;
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
                ContractRateId = company.ContractRateId
            };

            return new ResponseObjectDto<CompanyProfileDto>
            {
                IsSuccess = true,
                Message = "Company created successfully",
                StatusCode = 201,
                Data = result
            };
        }

        public async Task<ResponseObjectDto<List<CompanyProfileDto>>> GetAllCompaniesAsync()
        {
            var list = await _unitOfWork.CompanyProfiles.GetQueryable()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new CompanyProfileDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    TaxNumber = c.TaxNumber,
                    ContactPerson = c.ContactPerson,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address,
                    ContractRateId = c.ContractRateId
                })
                .ToListAsync();

            return new ResponseObjectDto<List<CompanyProfileDto>>
            {
                IsSuccess = true,
                Message = "Companies retrieved successfully",
                StatusCode = 200,
                Data = list
            };
        }
    }
}
