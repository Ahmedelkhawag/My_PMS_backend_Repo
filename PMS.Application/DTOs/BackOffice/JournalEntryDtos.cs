using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public record JournalEntryLineDto(int AccountId, decimal Debit, decimal Credit, string? Memo);

    public record CreateJournalEntryDto(string Description, DateTime Date, string? ReferenceNo, List<JournalEntryLineDto> Lines);
}

