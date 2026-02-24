using PMS.Domain.Enums;
using System;

namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Lightweight projection of a folio transaction for API responses.
    /// </summary>
    public record FolioTransactionDto
    {
        public int Id { get; init; }
        public int FolioId { get; init; }
        public DateTime Date { get; init; }
        public TransactionType Type { get; init; }
        public decimal Amount { get; init; }
        public string Description { get; init; } = string.Empty;
        public string? ReferenceNo { get; init; }
        public string? DiscountReason { get; init; }
        public bool IsVoided { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
