using PMS.Domain.Enums.BackOffice;

namespace PMS.Application.DTOs.BackOffice.AR
{
    public class TACommissionRecordDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int ReservationId { get; set; }
        public decimal EligibleRevenue { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal CommissionAmount { get; set; }
        public CommissionStatus Status { get; set; }
        public int? JournalEntryId { get; set; }
    }
}
