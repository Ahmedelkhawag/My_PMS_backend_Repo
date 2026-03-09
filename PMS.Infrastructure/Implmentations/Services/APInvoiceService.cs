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
    public class APInvoiceService : IAPInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<APInvoiceService> _logger;

        public APInvoiceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAccountingService accountingService,
            ILogger<APInvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountingService = accountingService;
            _logger = logger;
        }

        // ─── GET ALL ─────────────────────────────────────────────────────────────
        public async Task<PagedResult<APInvoiceDto>> GetAllInvoicesAsync(
            int pageNumber, int pageSize, int? vendorId)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize   = pageSize   < 1 ? 20 : pageSize;

            var query = _unitOfWork.APInvoices
                .GetQueryable()
                .Include(i => i.Vendor)
                .Include(i => i.Lines)
                .AsNoTracking();

            if (vendorId.HasValue)
                query = query.Where(i => i.VendorId == vendorId.Value);

            var totalCount = await query.CountAsync();
            var invoices   = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<APInvoiceDto>>(invoices);
            return new PagedResult<APInvoiceDto>(dtos, totalCount, pageNumber, pageSize);
        }

        // ─── GET BY ID ───────────────────────────────────────────────────────────
        public async Task<ApiResponse<APInvoiceDto>> GetInvoiceByIdAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.APInvoices
                .GetQueryable()
                .Include(i => i.Vendor)
                .Include(i => i.Lines)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                return new ApiResponse<APInvoiceDto>("AP Invoice not found.");

            return new ApiResponse<APInvoiceDto>(
                _mapper.Map<APInvoiceDto>(invoice), "AP Invoice retrieved successfully.");
        }

        // ─── CREATE ──────────────────────────────────────────────────────────────
        public async Task<ApiResponse<APInvoiceDto>> CreateInvoiceAsync(CreateAPInvoiceDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Vendor exists?
                var vendor = await _unitOfWork.Vendors
                    .GetQueryable()
                    .FirstOrDefaultAsync(v => v.Id == dto.VendorId);

                if (vendor == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<APInvoiceDto>("Vendor not found.");
                }

                // Duplicate vendor invoice number check
                var duplicate = await _unitOfWork.APInvoices
                    .GetQueryable()
                    .AnyAsync(i => i.VendorId == dto.VendorId
                               && i.VendorInvoiceNo == dto.VendorInvoiceNo);

                if (duplicate)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<APInvoiceDto>(
                        $"Invoice number '{dto.VendorInvoiceNo}' already exists for this vendor.");
                }

                var lines = dto.Lines?.ToList() ?? new List<CreateAPInvoiceLineDto>();
                if (!lines.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<APInvoiceDto>("Invoice must have at least one line.");
                }

                var totalAmount = lines.Sum(l => l.Amount);

                // Calculate DueDate from vendor credit terms
                var dueDate = dto.InvoiceDate.AddDays((int)vendor.CreditTerms);

                var invoice = new APInvoice
                {
                    VendorId        = dto.VendorId,
                    VendorInvoiceNo = dto.VendorInvoiceNo,
                    InvoiceDate     = dto.InvoiceDate,
                    DueDate         = dueDate,
                    TotalAmount     = totalAmount,
                    AmountPaid      = 0m,
                    Status          = APInvoiceStatus.Draft
                };

                foreach (var lineDto in lines)
                {
                    invoice.Lines.Add(new APInvoiceLine
                    {
                        Description      = lineDto.Description,
                        Amount           = lineDto.Amount,
                        ExpenseAccountId = lineDto.ExpenseAccountId
                    });
                }

                await _unitOfWork.APInvoices.AddAsync(invoice);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Map directly from in-memory entity — avoiding EF tracking conflict
                // after CommitTransactionAsync on the same DbContext scope.
                // Vendor is already loaded above; Lines were added to invoice.Lines in this scope.
                invoice.Vendor = vendor;

                var result = new APInvoiceDto
                {
                    Id = invoice.Id,
                    VendorId = invoice.VendorId,
                    VendorName = vendor.Name,
                    VendorInvoiceNo = invoice.VendorInvoiceNo,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    TotalAmount = invoice.TotalAmount,
                    AmountPaid = invoice.AmountPaid,
                    Balance = invoice.TotalAmount - invoice.AmountPaid,
                    Status = invoice.Status,
                    JournalEntryId = invoice.JournalEntryId,
                    Lines = invoice.Lines.Select(l => new APInvoiceLineDto
                    {
                        Id = l.Id,
                        APInvoiceId = l.APInvoiceId,
                        Description = l.Description,
                        Amount = l.Amount,
                        ExpenseAccountId = l.ExpenseAccountId
                    })
                };

                return new ApiResponse<APInvoiceDto>(result, "AP Invoice created as Draft successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating AP Invoice for vendor {VendorId}", dto.VendorId);
                return new ApiResponse<APInvoiceDto>("An unexpected error occurred while creating the AP Invoice.");
            }
        }

        // ─── APPROVE ─────────────────────────────────────────────────────────────
        public async Task<ApiResponse<APInvoiceDto>> ApproveInvoiceAsync(int invoiceId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.APInvoices
                    .GetQueryable()
                    .Include(i => i.Vendor)
                    .Include(i => i.Lines)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<APInvoiceDto>("AP Invoice not found.");
                }

                if (invoice.Status != APInvoiceStatus.Draft)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<APInvoiceDto>(
                        $"Only Draft invoices can be approved. Current status: {invoice.Status}.");
                }

                // Change status first so the GL call finds an updated entity
                invoice.Status = APInvoiceStatus.Approved;
                _unitOfWork.APInvoices.Update(invoice);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Post to GL (has its own inner transaction via BeginTransactionAsync depth counter)
                var glResult = await _accountingService.PostAPInvoiceToGLAsync(invoiceId);
                if (!glResult.Succeeded)
                {
                    _logger.LogWarning(
                        "AP Invoice {InvoiceId} approved but GL posting failed: {Msg}",
                        invoiceId, glResult.Message);
                }

                // Return updated invoice
                return await GetInvoiceByIdAsync(invoiceId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error approving AP Invoice {InvoiceId}", invoiceId);
                return new ApiResponse<APInvoiceDto>("An unexpected error occurred while approving the AP Invoice.");
            }
        }

        // ─── VOID ────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> VoidInvoiceAsync(int invoiceId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.APInvoices
                    .GetQueryable()
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("AP Invoice not found.");
                }

                if (invoice.Status == APInvoiceStatus.Paid ||
                    invoice.Status == APInvoiceStatus.PartiallyPaid)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>(
                        "Cannot void a Paid or Partially Paid invoice. Reverse the payments first.");
                }

                // If Approved, reverse the GL entry
                if (invoice.Status == APInvoiceStatus.Approved && invoice.JournalEntryId.HasValue)
                {
                    var reversalResult = await _accountingService.ReverseJournalEntryAsync(
                        invoice.JournalEntryId.Value,
                        $"VOID-INV-{invoiceId}",
                        $"Void of AP Invoice #{invoice.VendorInvoiceNo}");

                    if (!reversalResult.Succeeded)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<bool>(
                            $"Failed to reverse GL entry: {reversalResult.Message}");
                    }
                }

                invoice.Status = APInvoiceStatus.Voided;
                _unitOfWork.APInvoices.Update(invoice);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "AP Invoice voided successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error voiding AP Invoice {InvoiceId}", invoiceId);
                return new ApiResponse<bool>("An unexpected error occurred while voiding the AP Invoice.");
            }
        }
    }
}
