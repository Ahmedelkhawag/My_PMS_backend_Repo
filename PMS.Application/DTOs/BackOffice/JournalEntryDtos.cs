using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public record JournalEntryLineDto(
        int AccountId, 
        decimal Debit, 
        decimal Credit, 
        string? Memo, 
        int? CurrencyId = null, 
        decimal ExchangeRate = 1m, 
        decimal DebitForeign = 0m, 
        decimal CreditForeign = 0m,
        int? CostCenterId = null
    );

    public record CreateJournalEntryDto(string Description, DateTime Date, string? ReferenceNo, List<JournalEntryLineDto> Lines);
    
    public record RejectJournalEntryDto(string Reason);
}

