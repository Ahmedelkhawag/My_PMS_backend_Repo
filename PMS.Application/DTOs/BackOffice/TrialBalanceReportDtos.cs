using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public record TrialBalanceItemDto(string AccountCode, string AccountName, decimal Debit, decimal Credit);

    public record TrialBalanceReportDto(List<TrialBalanceItemDto> Items, decimal TotalDebit, decimal TotalCredit, bool IsBalanced);
}
