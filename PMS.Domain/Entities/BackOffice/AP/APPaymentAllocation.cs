using PMS.Domain.Entities.BackOffice;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AP
{
    public class APPaymentAllocation : BaseAuditableEntity
    {
        [Required]
        public int APPaymentId { get; set; }

        public virtual APPayment APPayment { get; set; } = null!;

        [Required]
        public int APInvoiceId { get; set; }

        public virtual APInvoice APInvoice { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; }
    }
}
