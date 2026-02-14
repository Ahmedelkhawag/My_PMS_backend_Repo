namespace PMS.Application.DTOs.Companies
{
    public class CompanyProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int? ContractRateId { get; set; }
    }
}
