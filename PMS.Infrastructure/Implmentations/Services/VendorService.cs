using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.BackOffice.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class VendorService : IVendorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VendorService> _logger;

        public VendorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VendorService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<VendorDto>> GetAllVendorsAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize   = pageSize <= 0 ? 20 : pageSize;

            var query = _unitOfWork.Vendors
                .GetQueryable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(v =>
                    v.Name.ToLower().Contains(term) ||
                    v.TaxId.ToLower().Contains(term) ||
                    (v.ContactPerson != null && v.ContactPerson.ToLower().Contains(term)) ||
                    (v.Email != null && v.Email.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var vendors = await query
                .OrderBy(v => v.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<VendorDto>>(vendors);
            return new PagedResult<VendorDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<ApiResponse<VendorDto>> GetVendorByIdAsync(int id)
        {
            var vendor = await _unitOfWork.Vendors
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendor == null)
                return new ApiResponse<VendorDto>("Vendor not found.");

            return new ApiResponse<VendorDto>(_mapper.Map<VendorDto>(vendor), "Vendor retrieved successfully.");
        }

        public async Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Rule 1.1: TaxId uniqueness
                var taxIdExists = await _unitOfWork.Vendors
                    .GetQueryable()
                    .AnyAsync(v => v.TaxId == dto.TaxId);

                if (taxIdExists)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<VendorDto>("A vendor with this Tax ID already exists.");
                }

                // Rule 1.3: GL Account validation
                var apAccountExists = await _unitOfWork.Accounts
                    .GetQueryable()
                    .AnyAsync(a => a.Id == dto.APAccountId);

                if (!apAccountExists)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<VendorDto>($"AP Account with Id {dto.APAccountId} not found in the Chart of Accounts.");
                }

                if (dto.DefaultExpenseAccountId.HasValue)
                {
                    var expAccExists = await _unitOfWork.Accounts
                        .GetQueryable()
                        .AnyAsync(a => a.Id == dto.DefaultExpenseAccountId.Value);

                    if (!expAccExists)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<VendorDto>($"Default Expense Account with Id {dto.DefaultExpenseAccountId} not found in the Chart of Accounts.");
                    }
                }

                var vendor = _mapper.Map<Vendor>(dto);
                await _unitOfWork.Vendors.AddAsync(vendor);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<VendorDto>(_mapper.Map<VendorDto>(vendor), "Vendor created successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating vendor with TaxId {TaxId}", dto.TaxId);
                return new ApiResponse<VendorDto>("An unexpected error occurred while creating the vendor.");
            }
        }

        public async Task<ApiResponse<VendorDto>> UpdateVendorAsync(int id, UpdateVendorDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var vendor = await _unitOfWork.Vendors
                    .GetQueryable()
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vendor == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<VendorDto>("Vendor not found.");
                }

                // Rule 1.1: TaxId uniqueness — only validate if TaxId was provided
                if (!string.IsNullOrWhiteSpace(dto.TaxId) && dto.TaxId != vendor.TaxId)
                {
                    var taxIdExists = await _unitOfWork.Vendors
                        .GetQueryable()
                        .AnyAsync(v => v.TaxId == dto.TaxId && v.Id != id);

                    if (taxIdExists)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<VendorDto>("A vendor with this Tax ID already exists.");
                    }

                    vendor.TaxId = dto.TaxId;
                }

                // Rule 1.3: AP Account validation — only if APAccountId was provided
                if (dto.APAccountId.HasValue)
                {
                    var apAccountExists = await _unitOfWork.Accounts
                        .GetQueryable()
                        .AnyAsync(a => a.Id == dto.APAccountId.Value);

                    if (!apAccountExists)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<VendorDto>($"AP Account with Id {dto.APAccountId} not found in the Chart of Accounts.");
                    }

                    vendor.APAccountId = dto.APAccountId.Value;
                }

                // Validate DefaultExpenseAccountId only if it was provided
                if (dto.DefaultExpenseAccountId.HasValue)
                {
                    var expAccExists = await _unitOfWork.Accounts
                        .GetQueryable()
                        .AnyAsync(a => a.Id == dto.DefaultExpenseAccountId.Value);

                    if (!expAccExists)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<VendorDto>($"Default Expense Account with Id {dto.DefaultExpenseAccountId} not found in the Chart of Accounts.");
                    }

                    vendor.DefaultExpenseAccountId = dto.DefaultExpenseAccountId.Value;
                }

                // Apply the remaining optional scalar fields only if provided
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    vendor.Name = dto.Name;

                if (dto.ContactPerson != null)
                    vendor.ContactPerson = dto.ContactPerson;

                if (dto.Email != null)
                    vendor.Email = dto.Email;

                if (dto.Phone != null)
                    vendor.Phone = dto.Phone;

                if (dto.CreditTerms.HasValue)
                    vendor.CreditTerms = dto.CreditTerms.Value;

                if (dto.IsActive.HasValue)
                    vendor.IsActive = dto.IsActive.Value;

                _unitOfWork.Vendors.Update(vendor);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<VendorDto>(_mapper.Map<VendorDto>(vendor), "Vendor updated successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating vendor {VendorId}", id);
                return new ApiResponse<VendorDto>("An unexpected error occurred while updating the vendor.");
            }
        }


        public async Task<ApiResponse<bool>> DeleteVendorAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var vendor = await _unitOfWork.Vendors
                    .GetQueryable()
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vendor == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("Vendor not found.");
                }

                // Rule 1.2: No hard delete if there are linked invoices or payments
                var hasInvoices = await _unitOfWork.APInvoices
                    .GetQueryable()
                    .AnyAsync(i => i.VendorId == id);

                var hasPayments = await _unitOfWork.APPayments
                    .GetQueryable()
                    .AnyAsync(p => p.VendorId == id);

                if (hasInvoices || hasPayments)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("Cannot delete vendor because there are associated invoices or payments. Consider deactivating instead.");
                }

                // Soft delete — set flags manually, do NOT call Remove()
                vendor.IsDeleted  = true;
                vendor.DeletedAt  = DateTime.UtcNow;
                vendor.IsActive   = false;

                _unitOfWork.Vendors.Update(vendor);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "Vendor deleted successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting vendor {VendorId}", id);
                return new ApiResponse<bool>("An unexpected error occurred while deleting the vendor.");
            }
        }
    }
}
