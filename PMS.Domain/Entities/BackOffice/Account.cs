using PMS.Domain.Entities;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice
{
    /// <summary>
    /// Represents a general ledger account in the Chart of Accounts.
    /// Supports a hierarchical structure through the ParentAccount relationship.
    /// </summary>
    public class Account : BaseAuditableEntity
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public int? ParentAccountId { get; set; }

        public int Level { get; set; }

        public bool IsGroup { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0m;

        public bool IsActive { get; set; } = true;

        public virtual Account? ParentAccount { get; set; }

        public virtual ICollection<Account> ChildAccounts { get; set; } = new List<Account>();

        /// <summary>
        /// Ensures that this account can accept postings (i.e., it is not a group/header account).
        /// Throws an InvalidOperationException if the account is a group.
        /// Application services should call this before creating journal entry lines.
        /// </summary>
        public void EnsureCanPost()
        {
            if (IsGroup)
            {
                throw new InvalidOperationException("Cannot post journal entries to a group account.");
            }
        }
    }
}

