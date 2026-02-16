using PMS.Domain.Enums;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{
    public class FolioTransaction : ISoftDeletable, IAuditable
    {
        public int Id { get; set; }

        [Required]
        public int FolioId { get; set; }

        [ForeignKey(nameof(FolioId))]
        public GuestFolio Folio { get; set; }

        /// <summary>
        /// The effective date/time of the transaction (UTC).
        /// </summary>
        public DateTime Date { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        /// <summary>
        /// Always stored as the signed amount that affects the folio.
        /// For normal transactions this is positive; for reversals it will be negative.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ReferenceNo { get; set; }

        public bool IsVoided { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}

