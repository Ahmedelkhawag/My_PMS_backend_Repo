using PMS.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice
{
    /// <summary>
    /// Represents a single debit or credit line within a journal entry.
    /// Lines must always post to non-group accounts and participate in the balancing of the parent journal entry.
    /// </summary>
    public class JournalEntryLine : BaseAuditableEntity
    {
        public int JournalEntryId { get; set; }

        public virtual JournalEntry JournalEntry { get; set; } = null!;

        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;

        public int? CostCenterId { get; set; }

        public virtual CostCenter? CostCenter { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; } = 0m;

        public int? CurrencyId { get; set; }

        public virtual PMS.Domain.Entities.Configuration.Currency? Currency { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRate { get; set; } = 1m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitForeign { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditForeign { get; set; } = 0m;

        [MaxLength(500)]
        public string? Memo { get; set; }
    }
}

