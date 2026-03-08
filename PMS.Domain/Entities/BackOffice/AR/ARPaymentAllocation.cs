using PMS.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AR
{
    public class ARPaymentAllocation : BaseAuditableEntity
    {
        [Required]
        public int ARPaymentId { get; set; }

        public virtual ARPayment ARPayment { get; set; } = null!;

        [Required]
        public int ARInvoiceId { get; set; }

        public virtual ARInvoice ARInvoice { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountApplied { get; set; }
    }
}

