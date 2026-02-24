using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Companies
{
    public record UpdateCompanyProfileDto
    {
        public string? Name { get; init; }
        public string? TaxNumber { get; init; }
        public string? ContactPerson { get; init; }

        [Phone]
        public string? PhoneNumber { get; init; }

        [EmailAddress]
        public string? Email { get; init; }

        public string? Address { get; init; }
        public int? ContractRateId { get; init; }
    }
}
