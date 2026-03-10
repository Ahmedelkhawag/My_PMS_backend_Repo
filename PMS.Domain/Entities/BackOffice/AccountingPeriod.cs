using System;
using PMS.Domain.Enums.BackOffice;

namespace PMS.Domain.Entities.BackOffice
{
    public class AccountingPeriod : BaseAuditableEntity
    {
        public int FiscalYearId { get; set; }
        public int MonthNumber { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public AccountingPeriodStatus Status { get; set; }

        public FiscalYear FiscalYear { get; set; }
    }
}
