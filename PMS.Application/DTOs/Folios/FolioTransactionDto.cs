using PMS.Domain.Enums;
using System;

namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Lightweight projection of a folio transaction for API responses.
    /// </summary>
    public class FolioTransactionDto
    {
        public int Id { get; set; }
        public int FolioId { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public string? DiscountReason { get; set; }
        public bool IsVoided { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

