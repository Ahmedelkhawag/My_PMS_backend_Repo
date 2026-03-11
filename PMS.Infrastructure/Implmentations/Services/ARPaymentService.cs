using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using PMS.Application.Exceptions;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.BackOffice.AR;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class ARPaymentService : IARPaymentService
    {
        private readonly IUnitOfWork _uof;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<ARPaymentService> _logger;

        public ARPaymentService(IUnitOfWork uof, IAccountingService accountingService, ILogger<ARPaymentService> logger)
        {
            _uof = uof;
            _accountingService = accountingService;
            _logger = logger;
        }

        public async Task<ApiResponse<int>> ProcessPaymentAsync(ProcessPaymentDto dto)
        {
            if (dto.Amount <= 0)
                throw new BusinessException("Payment amount must be greater than zero.");

            var company = await _uof.CompanyProfiles.GetByIdAsync(dto.CompanyId);
            if (company == null)
                throw new BusinessException("Company not found.");

            var businessDate = await _uof.GetCurrentBusinessDateAsync();

            var payment = new ARPayment
            {
                CompanyId = dto.CompanyId,
                Amount = dto.Amount,
                PaymentDate = businessDate,
                PaymentMethod = dto.Method.ToString(),
                ReferenceNumber = dto.ReferenceNo,
                Remarks = dto.Remarks,
                InvoiceId = dto.InvoiceId
            };

            await _uof.BeginTransactionAsync();
            try
            {
                ARAllocation? newAllocation = null;

                if (dto.InvoiceId == null)
                {
                    payment.UnallocatedAmount = dto.Amount;
                    payment.Status = PaymentStatus.Open;
                    
                    await _uof.ARPayments.AddAsync(payment);
                    await _uof.CompleteAsync();
                }
                else
                {
                    // If invoice is provided, automatically allocate it to the invoice
                    var invoice = await _uof.ARInvoices.GetByIdAsync(dto.InvoiceId.Value);
                    if (invoice == null)
                        throw new BusinessException("Invoice not found.");

                    var neededAmount = invoice.TotalAmount - invoice.PaidAmount;
                    if (neededAmount <= 0)
                        throw new BusinessException("Invoice is already fully paid.");

                    var allocationAmount = Math.Min(dto.Amount, neededAmount);
                    
                    payment.UnallocatedAmount = dto.Amount - allocationAmount;
                    payment.Status = payment.UnallocatedAmount == 0 ? PaymentStatus.Settled : PaymentStatus.Partial;
                    if (payment.UnallocatedAmount == payment.Amount)
                        payment.Status = PaymentStatus.Open;

                    await _uof.ARPayments.AddAsync(payment);
                    await _uof.CompleteAsync();

                    // Create Allocation
                    newAllocation = new ARAllocation
                    {
                        PaymentId = payment.Id,
                        InvoiceId = invoice.Id,
                        Amount = allocationAmount,
                        AllocatedDate = businessDate
                    };
                    await _uof.ARAllocations.AddAsync(newAllocation);

                    // Update Invoice
                    invoice.PaidAmount += allocationAmount;
                    invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? ARInvoiceStatus.Paid : ARInvoiceStatus.PartiallyPaid;
                    _uof.ARInvoices.Update(invoice);
                    
                    await _uof.CompleteAsync();
                }

                // Post to GL
                var glResult = await _accountingService.PostARPaymentToGLAsync(payment.Id);
                if (!glResult.Succeeded)
                {
                    _logger.LogError("Posting AR Payment {PaymentId} to GL failed: {Message}", payment.Id, glResult.Message);
                }

                if (newAllocation != null)
                {
                    var allocGlResult = await _accountingService.PostARAllocationToGLAsync(newAllocation.Id);
                    if (!allocGlResult.Succeeded)
                        _logger.LogError("Posting AR Allocation {AllocationId} to GL failed: {Message}", newAllocation.Id, allocGlResult.Message);
                }

                await _uof.CommitTransactionAsync();

                return new ApiResponse<int>(payment.Id, "Payment processed successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uof.RollbackTransactionAsync();
                throw new BusinessException("The record was updated by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                await _uof.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing payment for company {CompanyId}.", dto.CompanyId);
                if (ex is BusinessException) throw;
                throw new BusinessException("An error occurred while processing the payment.");
            }
        }

        public async Task<ApiResponse<bool>> AllocatePaymentAsync(int paymentId, List<AllocationRequest> requests)
        {
            if (requests == null || !requests.Any())
                throw new BusinessException("No allocation requests provided.");

            if (requests.Any(r => r.Amount <= 0))
                throw new BusinessException("All allocation amounts must be greater than zero.");

            await _uof.BeginTransactionAsync();
            try
            {
                var payment = await _uof.ARPayments.GetByIdAsync(paymentId);
                if (payment == null)
                    throw new BusinessException("Payment not found.");

                if (payment.Status == PaymentStatus.Settled || payment.Status == PaymentStatus.Voided)
                    throw new BusinessException("Cannot allocate from a settled or voided payment.");

                var totalRequestedAmount = requests.Sum(r => r.Amount);
                if (totalRequestedAmount > payment.UnallocatedAmount)
                    throw new BusinessException($"Total requested amount ({totalRequestedAmount}) exceeds the unallocated amount ({payment.UnallocatedAmount}).");

                var businessDate = await _uof.GetCurrentBusinessDateAsync();
                var createdAllocations = new List<ARAllocation>();

                foreach (var req in requests)
                {
                    var invoice = await _uof.ARInvoices.GetByIdAsync(req.InvoiceId);
                    if (invoice == null)
                        throw new BusinessException($"Invoice with ID {req.InvoiceId} not found.");

                    if (invoice.CompanyId != payment.CompanyId)
                        throw new BusinessException($"Invoice with ID {req.InvoiceId} belongs to a different company.");

                    var neededAmount = invoice.TotalAmount - invoice.PaidAmount;
                    if (req.Amount > neededAmount)
                        throw new BusinessException($"Allocation amount ({req.Amount}) for Invoice {req.InvoiceId} exceeds the remaining needed amount ({neededAmount}).");

                    // 1. Create Allocation record
                    var allocation = new ARAllocation
                    {
                        PaymentId = payment.Id,
                        InvoiceId = invoice.Id,
                        Amount = req.Amount,
                        AllocatedDate = businessDate
                    };
                    await _uof.ARAllocations.AddAsync(allocation);
                    createdAllocations.Add(allocation);

                    // 2. Update Invoice
                    invoice.PaidAmount += req.Amount;
                    invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? ARInvoiceStatus.Paid : ARInvoiceStatus.PartiallyPaid;
                    _uof.ARInvoices.Update(invoice);
                }

                // 3. Update Payment
                payment.UnallocatedAmount -= totalRequestedAmount;
                if (payment.UnallocatedAmount == 0)
                {
                    payment.Status = PaymentStatus.Settled;
                }
                else if (payment.UnallocatedAmount < payment.Amount)
                {
                    payment.Status = PaymentStatus.Partial;
                }
                
                _uof.ARPayments.Update(payment);

                await _uof.CompleteAsync();

                foreach (var alloc in createdAllocations)
                {
                    var glResult = await _accountingService.PostARAllocationToGLAsync(alloc.Id);
                    if (!glResult.Succeeded)
                        _logger.LogError("Posting AR Allocation {AllocationId} to GL failed: {Message}", alloc.Id, glResult.Message);
                }

                await _uof.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "Payment successfully allocated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uof.RollbackTransactionAsync();
                throw new BusinessException("The record was updated by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                await _uof.RollbackTransactionAsync();
                _logger.LogError(ex, "Error allocating payment {PaymentId}.", paymentId);
                if (ex is BusinessException) throw;
                throw new BusinessException("An error occurred while allocating the payment.");
            }
        }
    }
}
