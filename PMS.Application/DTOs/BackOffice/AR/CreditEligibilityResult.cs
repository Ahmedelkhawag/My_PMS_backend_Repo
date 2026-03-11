using PMS.Domain.Enums.BackOffice;

namespace PMS.Application.DTOs.BackOffice.AR
{
    public class CreditEligibilityResult
    {
        public bool IsEligible { get; set; }
        public CreditEligibilityStatus Status { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
