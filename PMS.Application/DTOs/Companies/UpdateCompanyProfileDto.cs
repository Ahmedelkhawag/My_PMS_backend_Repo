using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Companies
{
    public class UpdateCompanyProfileDto
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? TaxNumber { get; set; }

        public string? ContactPerson { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public int? ContractRateId { get; set; }
    }
}
