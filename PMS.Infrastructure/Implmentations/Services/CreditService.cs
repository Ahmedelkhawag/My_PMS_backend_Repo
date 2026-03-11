using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class CreditService : ICreditService
    {
        private readonly IUnitOfWork _uof;

        public CreditService(IUnitOfWork uof)
        {
            _uof = uof;
        }

        public async Task<CreditEligibilityResult> CheckCreditEligibilityAsync(int companyId, decimal newInvoiceAmount)
        {
            var company = await _uof.CompanyProfiles.GetByIdAsync(companyId);

            if (company == null)
            {
                return new CreditEligibilityResult
                {
                    IsEligible = false,
                    Status = CreditEligibilityStatus.CompanyNotFound,
                    CurrentBalance = 0
                };
            }

            if (!company.IsCreditEnabled)
            {
                return new CreditEligibilityResult
                {
                    IsEligible = false,
                    Status = CreditEligibilityStatus.CreditDisabled,
                    CurrentBalance = 0
                };
            }

            var currentDate = await _uof.GetCurrentBusinessDateAsync();

            // 1. Total Debt
            // Get open invoices using GetQueryable
            var openInvoicesQuery = _uof.ARInvoices.GetQueryable()
                .Where(i => i.CompanyId == companyId &&
                            i.Status != ARInvoiceStatus.Paid);

            decimal totalOpenInvoicesRemaining = await openInvoicesQuery
                .SumAsync(i => i.TotalAmount - i.PaidAmount);

            // Get unallocated payments
            var unallocatedPaymentsSum = await _uof.ARPayments.GetQueryable()
                .Where(p => p.CompanyId == companyId && p.Status != PaymentStatus.Voided)
                .SumAsync(p => p.UnallocatedAmount);

            decimal currentDebt = totalOpenInvoicesRemaining - unallocatedPaymentsSum;

            // 2. Limit Check
            if (currentDebt + newInvoiceAmount > company.CreditLimit)
            {
                return new CreditEligibilityResult
                {
                    IsEligible = false,
                    Status = CreditEligibilityStatus.CreditLimitExceeded,
                    CurrentBalance = currentDebt
                };
            }

            // 3. Aging Check
            bool hasOverdueInvoices = await openInvoicesQuery
                .AnyAsync(i => i.InvoiceDate.AddDays(company.CreditDays) < currentDate);

            if (hasOverdueInvoices)
            {
                return new CreditEligibilityResult
                {
                    IsEligible = false,
                    Status = CreditEligibilityStatus.OverdueInvoicesFound,
                    CurrentBalance = currentDebt
                };
            }

            // Eligible
            return new CreditEligibilityResult
            {
                IsEligible = true,
                Status = CreditEligibilityStatus.Eligible,
                CurrentBalance = currentDebt
            };
        }
    }
}
