using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Enums.BackOffice.AP;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AP
{
    public class APInvoice : BaseAuditableEntity
    {
        [Required]
        public int VendorId { get; set; }

        public virtual Vendor Vendor { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string VendorInvoiceNo { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; } = 0m;

        public APInvoiceStatus Status { get; set; } = APInvoiceStatus.Draft;

        public int? JournalEntryId { get; set; }

        public virtual ICollection<APInvoiceLine> Lines { get; set; } = new List<APInvoiceLine>();
        public virtual ICollection<APPaymentAllocation> Allocations { get; set; } = new List<APPaymentAllocation>();
    }
}
