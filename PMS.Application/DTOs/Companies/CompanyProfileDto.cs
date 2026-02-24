using PMS.Application.DTOs.Common;

namespace PMS.Application.DTOs.Companies
{
    public record CompanyProfileDto : BaseAuditableDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? TaxNumber { get; init; }
        public string ContactPerson { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Address { get; init; }
        public int? ContractRateId { get; init; }
    }
}
