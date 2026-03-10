using System;
using System.Collections.Generic;
using PMS.Domain.Enums.BackOffice;

namespace PMS.Domain.Entities.BackOffice
{
    public class FiscalYear : BaseAuditableEntity
    {
        public string Name { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public bool IsClosed { get; set; }

        public ICollection<AccountingPeriod> AccountingPeriods { get; set; } = new List<AccountingPeriod>();

        /// <summary>
        /// Logic to generate 12 months automatically when a Fiscal Year is created.
        /// </summary>
        public static FiscalYear CreateWithPeriods(string name, DateTimeOffset startDate)
        {
            var fiscalYear = new FiscalYear
            {
                Name = name,
                StartDate = startDate,
                EndDate = startDate.AddYears(1).AddDays(-1),
                IsClosed = false,
                AccountingPeriods = new List<AccountingPeriod>()
            };

            for (int i = 1; i <= 12; i++)
            {
                var periodStart = startDate.AddMonths(i - 1);
                var periodEnd = periodStart.AddMonths(1).AddDays(-1);

                fiscalYear.AccountingPeriods.Add(new AccountingPeriod
                {
                    MonthNumber = i,
                    StartDate = periodStart,
                    EndDate = periodEnd,
                    Status = i == 1 ? AccountingPeriodStatus.Open : AccountingPeriodStatus.Future
                });
            }

            return fiscalYear;
        }
    }
}
