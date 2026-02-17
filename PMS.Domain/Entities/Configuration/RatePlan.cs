using PMS.Domain.Enums;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities.Configuration
{
    public class RatePlan : IAuditable, ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // STANDARD, NONREF, CORP_VOD

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // Standard Rate, Non-Refundable

        public string? Description { get; set; }

        [Required]
        public RateType RateType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RateValue { get; set; } // percentage or value depending on RateType

        /// <summary>
        /// Is this plan visible for B2C / individual bookings?
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Business switch to deactivate the plan while keeping history.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Relationships
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<CompanyProfile> Companies { get; set; } = new List<CompanyProfile>();

        // Auditing
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}

