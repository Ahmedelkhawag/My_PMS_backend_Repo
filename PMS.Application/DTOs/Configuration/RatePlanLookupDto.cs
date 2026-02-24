namespace PMS.Application.DTOs.Configuration
{
    public record RatePlanLookupDto
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int RateType { get; init; }
        public decimal RateValue { get; init; }
        public bool IsPublic { get; init; }
    }
}
