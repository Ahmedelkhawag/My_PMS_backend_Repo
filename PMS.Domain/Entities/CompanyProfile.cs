using PMS.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

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

        public string? Address { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
