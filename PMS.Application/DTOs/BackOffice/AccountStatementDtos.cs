using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public record AccountStatementLineDto(DateTime Date, string EntryNumber, string Description, decimal Debit, decimal Credit, decimal RunningBalance);

    public record AccountStatementHeaderDto(string AccountCode, string AccountName, decimal OpeningBalance, decimal ClosingBalance, List<AccountStatementLineDto> Lines);
}
