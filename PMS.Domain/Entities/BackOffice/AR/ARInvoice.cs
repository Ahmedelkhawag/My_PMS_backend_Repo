using PMS.Domain.Entities;
using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AR
{
    public class ARInvoice : BaseAuditableEntity
    {
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        public CompanyProfile Company { get; set; } = null!;

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0m;

        public ARInvoiceStatus Status { get; set; } = ARInvoiceStatus.Draft;

        public virtual ICollection<ARInvoiceLine> Lines { get; set; } = new List<ARInvoiceLine>();
    }
}

