using PMS.Application.DTOs.BackOffice.AR;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces.Services
{
    public interface ICreditService
    {
        Task<CreditEligibilityResult> CheckCreditEligibilityAsync(int companyId, decimal newInvoiceAmount);
    }
}
