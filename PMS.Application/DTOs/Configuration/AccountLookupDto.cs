using System.Collections.Generic;

namespace PMS.Application.DTOs.Configuration
{
    /// <summary>
    /// Lookup DTO for a General Ledger Account entry.
    /// Used for dropdowns in AP/AR forms.
    /// </summary>
    public record AccountLookupDto
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
        public string NameAr { get; init; } = string.Empty;
        public string AccountType { get; init; } = string.Empty;
        public bool IsGroup { get; init; }
        public int? ParentAccountId { get; init; }
    }
}
