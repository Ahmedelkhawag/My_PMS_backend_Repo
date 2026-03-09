using PMS.Domain.Entities.BackOffice;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AP
{
    public class APInvoiceLine : BaseAuditableEntity
    {
        [Required]
        public int APInvoiceId { get; set; }

        public virtual APInvoice APInvoice { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public int ExpenseAccountId { get; set; }

        public virtual Account ExpenseAccount { get; set; } = null!;
    }
}
