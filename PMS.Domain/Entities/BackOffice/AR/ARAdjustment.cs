using PMS.Domain.Entities;
using PMS.Domain.Enums.BackOffice;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AR
{
    public class ARAdjustment : BaseAuditableEntity
    {
        [Required]
        public int ARInvoiceId { get; set; }

        public virtual ARInvoice ARInvoice { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public ARAdjustmentType Type { get; set; }

        public DateTime AdjustmentDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }
    }
}
