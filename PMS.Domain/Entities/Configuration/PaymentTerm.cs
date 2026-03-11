using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities.Configuration
{
    /// <summary>
    /// Defines a named set of payment due-date rules (e.g. "Net 30", "Net 60").
    /// Linked to a CompanyProfile to drive DueDate calculation on ARInvoice.
    /// </summary>
    public class PaymentTerm : BaseAuditableEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Number of calendar days after the invoice date that the balance is due.
        /// </summary>
        [Required]
        public int Days { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
