using PMS.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AR
{
    public class ARInvoiceLine : BaseAuditableEntity
    {
        [Required]
        public int ARInvoiceId { get; set; }

        public virtual ARInvoice ARInvoice { get; set; } = null!;

        [Required]
        public int FolioTransactionId { get; set; }

        public virtual FolioTransaction FolioTransaction { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}

