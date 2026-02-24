using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Configuration
{
    public record RatePlanDto
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public RateType RateType { get; init; }
        public decimal RateValue { get; init; }
        public bool IsPublic { get; init; }
        public bool IsActive { get; init; }
    }
}
