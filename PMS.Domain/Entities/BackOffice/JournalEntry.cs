using PMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PMS.Domain.Enums.BackOffice;

namespace PMS.Domain.Entities.BackOffice
{
    /// <summary>
    /// Represents the header of a journal entry in the general ledger.
    /// A valid journal entry must have total debits equal to total credits across its lines.
    /// </summary>
    public class JournalEntry : BaseAuditableEntity
    {
        [Required]
        [MaxLength(50)]
        public string EntryNumber { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Posted;

        [MaxLength(450)]
        public string? ApprovedById { get; set; }

        public DateTimeOffset? ApprovedDate { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public int BusinessDayId { get; set; }

        public virtual BusinessDay BusinessDay { get; set; } = null!;

        public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();

        /// <summary>
        /// Determines whether this journal entry is balanced.
        /// A journal entry is considered balanced when the sum of debits equals the sum of credits across all lines.
        /// </summary>
        public bool IsBalanced()
        {
            decimal totalDebit = 0m;
            decimal totalCredit = 0m;

            foreach (var line in Lines)
            {
                totalDebit += line.Debit;
                totalCredit += line.Credit;
            }

            return totalDebit == totalCredit;
        }
    }
}

