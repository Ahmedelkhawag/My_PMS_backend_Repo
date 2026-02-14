using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Companies
{
    public class UpdateCompanyProfileDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        public string Name { get; set; } = string.Empty;

        public string? TaxNumber { get; set; }

        [Required(ErrorMessage = "Contact person is required")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        public int? ContractRateId { get; set; }
    }
}
