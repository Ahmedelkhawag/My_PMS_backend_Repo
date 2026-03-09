using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AP
{
    public class APPayment : BaseAuditableEntity
    {
        [Required]
        public int VendorId { get; set; }

        public virtual Vendor Vendor { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public ARPaymentMethod Method { get; set; }

        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        public int? JournalEntryId { get; set; }

        public bool IsVoided { get; set; } = false;

        public virtual ICollection<APPaymentAllocation> Allocations { get; set; } = new List<APPaymentAllocation>();
    }
}
