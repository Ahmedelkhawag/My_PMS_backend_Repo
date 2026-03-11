using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AR
{
    public class ARAllocation : BaseAuditableEntity
    {
        [Required]
        public int PaymentId { get; set; }
        public virtual ARPayment Payment { get; set; } = null!;

        [Required]
        public int InvoiceId { get; set; }
        public virtual ARInvoice Invoice { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime AllocatedDate { get; set; }
    }
}
