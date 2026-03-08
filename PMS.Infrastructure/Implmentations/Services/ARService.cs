using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Entities.BackOffice.AR;
using PMS.Domain.Enums;
using PMS.Domain.Enums.BackOffice;
using PMS.Infrastructure.Documents;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class ARService : IARService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<ARService> _logger;

        public ARService(IUnitOfWork unitOfWork, IAccountingService accountingService, ILogger<ARService> logger)
        {
            _unitOfWork = unitOfWork;
            _accountingService = accountingService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> TransferFolioToARAsync(TransferFolioDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var reservation = await _unitOfWork.Reservations
                    .GetQueryable()
                    .Include(r => r.GuestFolio)
                        .ThenInclude(f => f.Transactions)
                    .FirstOrDefaultAsync(r => r.Id == dto.ReservationId);

                if (reservation == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("Reservation not found.");
                }

                if (reservation.CompanyId == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("Reservation does not have a linked company to transfer to AR.");
                }

                var folio = reservation.GuestFolio;
                if (folio == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("Guest folio not found for this reservation.");
                }

                if (folio.Balance <= 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("There is no outstanding balance to transfer to AR.");
                }

                var chargeTransactions = folio.Transactions
                    .Where(t => !t.IsVoided && IsDebit(t.Type))
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id)
                    .ToList();

                if (!chargeTransactions.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>("No charge transactions found to transfer to AR.");
                }

                var outstandingAmount = folio.Balance;

                var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

                var invoice = new ARInvoice
                {
                    CompanyId = reservation.CompanyId.Value,
                    InvoiceDate = businessDate,
                    DueDate = businessDate,
                    TotalAmount = outstandingAmount,
                    PaidAmount = 0m,
                    Status = ARInvoiceStatus.Draft,
                    InvoiceNumber = await GenerateNextInvoiceNumberAsync(businessDate.Year)
                };

                await _unitOfWork.ARInvoices.AddAsync(invoice);

                foreach (var tx in chargeTransactions)
                {
                    var line = new ARInvoiceLine
                    {
                        ARInvoice = invoice,
                        FolioTransactionId = tx.Id,
                        Amount = tx.Amount,
                        Description = tx.Description
                    };

                    await _unitOfWork.ARInvoiceLines.AddAsync(line);
                }

                var cityLedgerPaymentAmount = outstandingAmount;

                var cityLedgerPayment = new FolioTransaction
                {
                    FolioId = folio.Id,
                    Date = DateTime.UtcNow,
                    BusinessDate = businessDate,
                    Type = TransactionType.CityLedgerPayment,
                    Amount = cityLedgerPaymentAmount,
                    Description = dto.Remarks ?? "Transfer to City Ledger (AR)",
                    ReferenceNo = invoice.InvoiceNumber,
                    IsVoided = false,
                    ShiftId = null
                };

                await _unitOfWork.FolioTransactions.AddAsync(cityLedgerPayment);

                folio.TotalPayments += cityLedgerPaymentAmount;
                folio.Balance -= cityLedgerPaymentAmount;
                _unitOfWork.GuestFolios.Update(folio);

                await _unitOfWork.CommitTransactionAsync();

                var glResult = await _accountingService.PostTransactionToGLAsync(cityLedgerPayment.Id);
                if (!glResult.Succeeded)
                {
                    _logger.LogError("Posting CityLedgerPayment {TransactionId} to GL failed: {Message}", cityLedgerPayment.Id, glResult.Message);
                    return new ApiResponse<bool>(glResult.Message);
                }

                return new ApiResponse<bool>(true, "Folio balance transferred to AR and posted to GL successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error while transferring folio {ReservationId} to AR.", dto.ReservationId);
                return new ApiResponse<bool>("An error occurred while transferring folio to AR.");
            }
        }

        private static bool IsDebit(TransactionType type)
        {
            var code = (int)type;
            return code >= 10 && code <= 19;
        }

        public async Task<ApiResponse<bool>> ReceiveARPaymentAsync(ReceiveARPaymentDto dto)
        {
            if (dto.Amount <= 0)
            {
                return new ApiResponse<bool>("Payment amount must be greater than zero.");
            }

            var company = await _unitOfWork.CompanyProfiles.GetByIdAsync(dto.CompanyId);
            if (company == null)
            {
                return new ApiResponse<bool>("Company not found.");
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var paymentMethodString = dto.Method.ToString();

            var payment = new ARPayment
            {
                CompanyId = dto.CompanyId,
                Amount = dto.Amount,
                PaymentDate = businessDate,
                PaymentMethod = paymentMethodString,
                ReferenceNumber = dto.ReferenceNo,
                Remarks = dto.Remarks,
                UnallocatedAmount = dto.Amount
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ARPayments.AddAsync(payment);
                await _unitOfWork.CompleteAsync();

                var outstandingInvoices = await _unitOfWork.ARInvoices
                    .GetQueryable()
                    .Where(i => i.CompanyId == dto.CompanyId && i.Status != ARInvoiceStatus.Paid)
                    .OrderBy(i => i.InvoiceDate)
                    .ToListAsync();

                var remainingPayment = dto.Amount;

                foreach (var invoice in outstandingInvoices)
                {
                    if (remainingPayment <= 0)
                        break;

                    var neededAmount = invoice.TotalAmount - invoice.PaidAmount;
                    if (neededAmount <= 0)
                        continue;

                    var allocationAmount = Math.Min(remainingPayment, neededAmount);

                    invoice.PaidAmount += allocationAmount;
                    invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? ARInvoiceStatus.Paid : ARInvoiceStatus.PartiallyPaid;
                    _unitOfWork.ARInvoices.Update(invoice);

                    var allocation = new ARPaymentAllocation
                    {
                        ARPaymentId = payment.Id,
                        ARInvoiceId = invoice.Id,
                        AmountApplied = allocationAmount
                    };
                    await _unitOfWork.ARPaymentAllocations.AddAsync(allocation);

                    remainingPayment -= allocationAmount;
                }

                payment.UnallocatedAmount = remainingPayment;
                _unitOfWork.ARPayments.Update(payment);

                await _unitOfWork.CommitTransactionAsync();

                var glResult = await _accountingService.PostARPaymentToGLAsync(payment.Id);
                if (!glResult.Succeeded)
                {
                    _logger.LogError("Posting AR Payment {PaymentId} to GL failed: {Message}", payment.Id, glResult.Message);
                    return new ApiResponse<bool>(glResult.Message);
                }

                return new ApiResponse<bool>(true, "Payment received and allocated successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error while receiving AR payment for company {CompanyId}.", dto.CompanyId);
                return new ApiResponse<bool>("An error occurred while receiving the payment.");
            }
        }

        public async Task<ApiResponse<CompanyStatementReportDto>> GetCompanyStatementAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            var company = await _unitOfWork.CompanyProfiles.GetByIdAsync(companyId);
            if (company == null)
            {
                return new ApiResponse<CompanyStatementReportDto>("Company not found.");
            }

            var invoicesQuery = _unitOfWork.ARInvoices
                .GetQueryable()
                .AsNoTracking()
                .Include(i => i.Lines)
                .Where(i => i.CompanyId == companyId);
            var paymentsQuery = _unitOfWork.ARPayments
                .GetQueryable()
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId);

            var totalInvoicesBefore = await invoicesQuery
                .Where(i => i.InvoiceDate < startDate)
                .SumAsync(i => i.TotalAmount);
            var totalPaymentsBefore = await paymentsQuery
                .Where(p => p.PaymentDate < startDate)
                .SumAsync(p => p.Amount);
            var openingBalance = totalInvoicesBefore - totalPaymentsBefore;

            var invoicesInRange = await invoicesQuery
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                .OrderBy(i => i.InvoiceDate)
                .ThenBy(i => i.Id)
                .ToListAsync();

            var paymentsInRange = await paymentsQuery
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderBy(p => p.PaymentDate)
                .ThenBy(p => p.Id)
                .ToListAsync();

            var invoiceLines = invoicesInRange.Select(i =>
            {
                var description = i.Lines?.FirstOrDefault()?.Description ?? $"Invoice {i.InvoiceNumber}";
                return new CompanyStatementLineDto(
                    i.InvoiceDate,
                    i.InvoiceNumber,
                    description,
                    i.TotalAmount,
                    0m,
                    0m
                );
            }).ToList();

            var paymentLines = paymentsInRange.Select(p =>
            {
                var description = $"Payment - {p.PaymentMethod}";
                if (!string.IsNullOrWhiteSpace(p.Remarks))
                    description += $" - {p.Remarks}";
                return new CompanyStatementLineDto(
                    p.PaymentDate,
                    p.ReferenceNumber ?? string.Empty,
                    description,
                    0m,
                    p.Amount,
                    0m
                );
            }).ToList();

            var allLines = invoiceLines
                .Concat(paymentLines)
                .OrderBy(l => l.Date)
                .ToList();

            var currentBalance = openingBalance;
            var linesWithBalance = new List<CompanyStatementLineDto>();
            foreach (var line in allLines)
            {
                currentBalance = currentBalance + line.Debit - line.Credit;
                linesWithBalance.Add(line with { Balance = currentBalance });
            }

            var report = new CompanyStatementReportDto(
                company.Name,
                startDate,
                endDate,
                openingBalance,
                currentBalance,
                linesWithBalance
            );

            return new ApiResponse<CompanyStatementReportDto>(report, "Company statement retrieved successfully.");
        }

        public async Task<ApiResponse<ARAgingReportDto>> GetARAgingReportAsync()
        {
            var today = DateTime.Today;
            var unpaidInvoices = await _unitOfWork.ARInvoices
                .GetQueryable()
                .AsNoTracking()
                .Include(i => i.Company)
                .Where(i => i.Status != ARInvoiceStatus.Paid)
                .ToListAsync();

            var groupedByCompany = unpaidInvoices
                .GroupBy(i => new { i.CompanyId, CompanyName = i.Company.Name })
                .ToList();

            var buckets = new List<ARAgingBucketDto>();

            foreach (var group in groupedByCompany)
            {
                decimal current0to30 = 0m;
                decimal over30 = 0m;
                decimal over60 = 0m;
                decimal over90 = 0m;

                foreach (var invoice in group)
                {
                    var remaining = invoice.TotalAmount - invoice.PaidAmount;
                    if (remaining <= 0)
                        continue;

                    var age = (today - invoice.InvoiceDate.Date).Days;

                    if (age <= 30)
                        current0to30 += remaining;
                    else if (age <= 60)
                        over30 += remaining;
                    else if (age <= 90)
                        over60 += remaining;
                    else
                        over90 += remaining;
                }

                var totalOutstanding = current0to30 + over30 + over60 + over90;
                if (totalOutstanding <= 0)
                    continue;

                buckets.Add(new ARAgingBucketDto(
                    group.Key.CompanyId,
                    group.Key.CompanyName,
                    current0to30,
                    over30,
                    over60,
                    over90,
                    totalOutstanding
                ));
            }

            var sortedBuckets = buckets
                .OrderByDescending(b => b.TotalOutstanding)
                .ToList();

            var grandTotal = sortedBuckets.Sum(b => b.TotalOutstanding);

            var report = new ARAgingReportDto(
                DateTime.UtcNow,
                sortedBuckets,
                grandTotal
            );

            return new ApiResponse<ARAgingReportDto>(report, "AR aging report generated successfully.");
        }

        public async Task<ApiResponse<CompanySOAPdfResultDto>> GenerateCompanySOAInPdfAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            var result = await GetCompanyStatementAsync(companyId, startDate, endDate);

            if (!result.Succeeded)
            {
                return new ApiResponse<CompanySOAPdfResultDto>(result.Message);
            }

            var document = new CompanySOADocument(result.Data!);
            var pdfBytes = Document.Create(document.Compose).GeneratePdf();
            var pdfResult = new CompanySOAPdfResultDto(pdfBytes, result.Data.CompanyName);

            return new ApiResponse<CompanySOAPdfResultDto>(pdfResult, "PDF generated successfully.");
        }

        public async Task<ApiResponse<bool>> CreateAdjustmentAsync(CreateARAdjustmentDto dto)
        {
            if (dto.Amount <= 0)
            {
                return new ApiResponse<bool>("Adjustment amount must be greater than zero.");
            }

            var invoice = await _unitOfWork.ARInvoices
                .GetQueryable()
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

            if (invoice == null)
            {
                return new ApiResponse<bool>("Invoice not found.");
            }

            if (dto.Type == ARAdjustmentType.CreditNote)
            {
                if (invoice.Status == ARInvoiceStatus.Paid)
                {
                    return new ApiResponse<bool>("Cannot apply a credit note to an invoice that is already paid.");
                }

                var remaining = invoice.TotalAmount - invoice.PaidAmount;
                if (dto.Amount > remaining)
                {
                    return new ApiResponse<bool>($"Credit note amount ({dto.Amount}) exceeds the remaining invoice balance ({remaining}).");
                }
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var referenceNumber = await GenerateAdjustmentReferenceAsync(dto.Type, businessDate.Year);

            var adjustment = new ARAdjustment
            {
                ARInvoiceId = dto.InvoiceId,
                Amount = dto.Amount,
                Type = dto.Type,
                AdjustmentDate = businessDate,
                Reason = dto.Reason,
                ReferenceNumber = referenceNumber
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ARAdjustments.AddAsync(adjustment);
                await _unitOfWork.CompleteAsync();

                if (dto.Type == ARAdjustmentType.CreditNote)
                {
                    invoice.TotalAmount -= dto.Amount;
                }
                else
                {
                    invoice.TotalAmount += dto.Amount;
                }

                invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? ARInvoiceStatus.Paid : ARInvoiceStatus.PartiallyPaid;
                _unitOfWork.ARInvoices.Update(invoice);

                await _unitOfWork.CommitTransactionAsync();

                var glResult = await _accountingService.PostARAdjustmentToGLAsync(adjustment.Id);
                if (!glResult.Succeeded)
                {
                    _logger.LogError("Posting AR Adjustment {AdjustmentId} to GL failed: {Message}", adjustment.Id, glResult.Message);
                    return new ApiResponse<bool>(glResult.Message);
                }

                return new ApiResponse<bool>(true, "AR adjustment created and posted to GL successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error while creating AR adjustment for invoice {InvoiceId}.", dto.InvoiceId);
                return new ApiResponse<bool>("An error occurred while creating the adjustment.");
            }
        }

        private async Task<string> GenerateAdjustmentReferenceAsync(ARAdjustmentType type, int year)
        {
            var prefix = type == ARAdjustmentType.CreditNote ? "CN-" : "DN-";
            var fullPrefix = $"{prefix}{year}-";

            var lastRef = await _unitOfWork.ARAdjustments
                .GetQueryable()
                .AsNoTracking()
                .Where(a => a.ReferenceNumber != null && a.ReferenceNumber.StartsWith(fullPrefix))
                .OrderByDescending(a => a.ReferenceNumber)
                .Select(a => a.ReferenceNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (!string.IsNullOrWhiteSpace(lastRef) &&
                int.TryParse(lastRef.Substring(fullPrefix.Length), out var parsed))
            {
                sequence = parsed + 1;
            }

            return $"{fullPrefix}{sequence:D4}";
        }

        private async Task<string> GenerateNextInvoiceNumberAsync(int year)
        {
            var prefix = $"INV-AR-{year}-";

            var lastNumber = await _unitOfWork.ARInvoices
                .GetQueryable()
                .AsNoTracking()
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber) &&
                int.TryParse(lastNumber.Substring(prefix.Length), out var parsed))
            {
                sequence = parsed + 1;
            }

            return $"{prefix}{sequence:D4}";
        }
    }
}

