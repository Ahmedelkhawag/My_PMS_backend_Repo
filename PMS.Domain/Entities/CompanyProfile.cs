using PMS.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using PMS.Domain.Entities.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
namespace PMS.Domain.Entities
{
    public class CompanyProfile : IAuditable, ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? TaxNumber { get; set; }

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int? ContractRateId { get; set; }

        /// <summary>
        /// Linked RatePlan for this company (takes precedence over public rate plans).
        /// </summary>
        public int? RatePlanId { get; set; }
        public RatePlan? RatePlan { get; set; }

        public string? Address { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; }

        public int CreditDays { get; set; }

        public bool IsCreditEnabled { get; set; }

        // ── Stage 2: Payment Terms ───────────────────────────────────────────
        /// <summary>Optional named payment term (e.g. "Net 30"). Overrides bare CreditDays when set.</summary>
        public int? PaymentTermId { get; set; }
        public virtual PaymentTerm? PaymentTerm { get; set; }

        // ── Stage 3: TA Commissions ──────────────────────────────────────────
        /// <summary>Commission percentage owed to this Travel Agent / OTA (e.g. 15.00 = 15%). Null means no commission.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CommissionRate { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
