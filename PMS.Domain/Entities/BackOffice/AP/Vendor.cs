using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Enums.BackOffice.AP;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AP
{
    public class Vendor : BaseAuditableEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TaxId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        public VendorCreditTerm CreditTerms { get; set; } = VendorCreditTerm.Net30;

        [Required]
        public int APAccountId { get; set; }

        public virtual Account APAccount { get; set; } = null!;

        public int? DefaultExpenseAccountId { get; set; }

        public virtual Account? DefaultExpenseAccount { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<APInvoice> Invoices { get; set; } = new List<APInvoice>();
        public virtual ICollection<APPayment> Payments { get; set; } = new List<APPayment>();
    }
}
