using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Companies
{
    public record CreateCompanyProfileDto
    {
        [Required(ErrorMessage = "Company name is required")]
        public string Name { get; init; } = string.Empty;

        public string? TaxNumber { get; init; }

        [Required(ErrorMessage = "Contact person is required")]
        public string ContactPerson { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string PhoneNumber { get; init; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        public string? Address { get; init; }

        public int? ContractRateId { get; init; }
    }
}
