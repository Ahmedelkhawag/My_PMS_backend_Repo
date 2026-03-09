using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.BackOffice.AP;
using PMS.Domain.Enums.BackOffice.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class APPaymentService : IAPPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountingService _accountingService;
        private readonly IMapper _mapper;
        private readonly ILogger<APPaymentService> _logger;

        public APPaymentService(
            IUnitOfWork unitOfWork,
            IAccountingService accountingService,
            IMapper mapper,
            ILogger<APPaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _accountingService = accountingService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<APPaymentDto>> CreatePaymentAsync(CreateAPPaymentDto dto)
        {
            if (dto.Amount <= 0)
            {
                return new ApiResponse<APPaymentDto>("Payment amount must be greater than zero.");
            }

            if (dto.Allocations == null || dto.Allocations.Count == 0)
            {
                return new ApiResponse<APPaymentDto>("At least one allocation is required.");
            }

            var totalAllocated = dto.Allocations.Sum(a => a.AllocatedAmount);
            if (totalAllocated != dto.Amount)
            {
                return new ApiResponse<APPaymentDto>("Sum of allocations must equal the payment amount.");
            }

            var vendor = await _unitOfWork.Vendors
                .GetQueryable()
                .FirstOrDefaultAsync(v => v.Id == dto.VendorId);

            if (vendor == null)
            {
                return new ApiResponse<APPaymentDto>("Vendor not found.");
            }

            var allocationInvoiceIds = dto.Allocations.Select(a => a.InvoiceId).Distinct().ToList();

            var invoices = await _unitOfWork.APInvoices
                .GetQueryable()
                .Where(i => allocationInvoiceIds.Contains(i.Id))
                .ToListAsync();

            if (invoices.Count != allocationInvoiceIds.Count)
            {
                return new ApiResponse<APPaymentDto>("One or more allocated invoices were not found.");
            }

            foreach (var invoice in invoices)
            {
                if (invoice.VendorId != dto.VendorId)
                {
                    return new ApiResponse<APPaymentDto>($"Invoice {invoice.Id} does not belong to the specified vendor.");
                }

                if (invoice.Status == APInvoiceStatus.Voided)
                {
                    return new ApiResponse<APPaymentDto>($"Invoice {invoice.Id} is voided and cannot be paid.");
                }

                if (invoice.Status == APInvoiceStatus.Draft)
                {
                    return new ApiResponse<APPaymentDto>($"Invoice {invoice.Id} is still Draft. Approve the invoice before applying payments.");
                }
            }

            foreach (var alloc in dto.Allocations)
            {
                var invoice = invoices.First(i => i.Id == alloc.InvoiceId);
                var remaining = invoice.TotalAmount - invoice.AmountPaid;
                if (alloc.AllocatedAmount <= 0)
                {
                    return new ApiResponse<APPaymentDto>($"Allocated amount for invoice {invoice.Id} must be greater than zero.");
                }

                if (alloc.AllocatedAmount > remaining)
                {
                    return new ApiResponse<APPaymentDto>($"Allocated amount for invoice {invoice.Id} exceeds the remaining balance ({remaining}).");
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

                var payment = new APPayment
                {
                    VendorId = dto.VendorId,
                    Amount = dto.Amount,
                    PaymentDate = businessDate,
                    Method = dto.Method,
                    ReferenceNo = dto.ReferenceNo
                };

                await _unitOfWork.APPayments.AddAsync(payment);
                await _unitOfWork.CompleteAsync(); // generate payment.Id

                var allocations = new List<APPaymentAllocation>();

                foreach (var alloc in dto.Allocations)
                {
                    var invoice = invoices.First(i => i.Id == alloc.InvoiceId);

                    invoice.AmountPaid += alloc.AllocatedAmount;
                    invoice.Status = invoice.AmountPaid >= invoice.TotalAmount
                        ? APInvoiceStatus.Paid
                        : APInvoiceStatus.PartiallyPaid;
                    _unitOfWork.APInvoices.Update(invoice);

                    allocations.Add(new APPaymentAllocation
                    {
                        APPaymentId = payment.Id,
                        APInvoiceId = invoice.Id,
                        AllocatedAmount = alloc.AllocatedAmount
                    });
                }

                await _unitOfWork.APPaymentAllocations.AddRangeAsync(allocations);

                await _unitOfWork.CommitTransactionAsync();

                var glResult = await _accountingService.PostAPPaymentToGLAsync(payment.Id, dto.CreditAccountId);
                if (!glResult.Succeeded)
                {
                    _logger.LogError("Posting AP Payment {PaymentId} to GL failed: {Message}", payment.Id, glResult.Message);
                    return new ApiResponse<APPaymentDto>(glResult.Message);
                }

                return await GetPaymentByIdAsync(payment.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error while creating AP payment for vendor {VendorId}.", dto.VendorId);
                return new ApiResponse<APPaymentDto>("An unexpected error occurred while creating the AP payment.");
            }
        }

        public async Task<ApiResponse<bool>> VoidPaymentAsync(int paymentId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var payment = await _unitOfWork.APPayments
                    .GetQueryable()
                    .Include(p => p.Allocations)
                        .ThenInclude(a => a.APInvoice)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("AP Payment not found.");
                }

                if (payment.IsVoided)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("AP Payment is already voided.");
                }

                if (payment.JournalEntryId.HasValue)
                {
                    var reversalResult = await _accountingService.ReverseJournalEntryAsync(
                        payment.JournalEntryId.Value,
                        $"VOID-APPAY-{payment.Id}",
                        $"Void of AP Payment #{payment.Id} - Ref: {payment.ReferenceNo ?? "N/A"}");

                    if (!reversalResult.Succeeded)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<bool>($"Failed to reverse GL entry: {reversalResult.Message}");
                    }
                }

                foreach (var allocation in payment.Allocations)
                {
                    var invoice = allocation.APInvoice;
                    if (invoice == null)
                        continue;

                    invoice.AmountPaid -= allocation.AllocatedAmount;
                    if (invoice.AmountPaid < 0)
                    {
                        invoice.AmountPaid = 0;
                    }

                    if (invoice.AmountPaid == 0)
                    {
                        invoice.Status = APInvoiceStatus.Approved;
                    }
                    else
                    {
                        invoice.Status = APInvoiceStatus.PartiallyPaid;
                    }

                    _unitOfWork.APInvoices.Update(invoice);
                }

                payment.IsVoided = true;
                _unitOfWork.APPayments.Update(payment);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "AP Payment voided successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error while voiding AP payment {PaymentId}.", paymentId);
                return new ApiResponse<bool>("An unexpected error occurred while voiding the AP payment.");
            }
        }

        public async Task<ApiResponse<APPaymentDto>> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _unitOfWork.APPayments
                .GetQueryable()
                .Include(p => p.Vendor)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.APInvoice)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return new ApiResponse<APPaymentDto>("AP Payment not found.");
            }

            var dto = _mapper.Map<APPaymentDto>(payment);
            return new ApiResponse<APPaymentDto>(dto, "AP Payment retrieved successfully.");
        }

        public async Task<PagedResult<APPaymentDto>> GetAllPaymentsAsync(int pageNumber, int pageSize, int? vendorId)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 20 : pageSize;

            var query = _unitOfWork.APPayments
                .GetQueryable()
                .Include(p => p.Vendor)
                .Include(p => p.Allocations)
                .AsNoTracking();

            if (vendorId.HasValue)
            {
                query = query.Where(p => p.VendorId == vendorId.Value);
            }

            var totalCount = await query.CountAsync();
            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<APPaymentDto>>(payments);
            return new PagedResult<APPaymentDto>(dtos, totalCount, pageNumber, pageSize);
        }
    }
}

