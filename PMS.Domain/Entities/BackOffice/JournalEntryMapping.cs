using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities.BackOffice
{
    /// <summary>
    /// Defines how a front-office folio transaction type should be posted to the general ledger.
    /// Maps a TransactionType to a debit and credit account.
    /// </summary>
    public class JournalEntryMapping : BaseAuditableEntity
    {
        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public int DebitAccountId { get; set; }

        [Required]
        public int CreditAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Account DebitAccount { get; set; } = null!;

        public virtual Account CreditAccount { get; set; } = null!;
    }
}

