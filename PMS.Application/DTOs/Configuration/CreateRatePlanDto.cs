using System.ComponentModel.DataAnnotations;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Configuration
{
    public record CreateRatePlanDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; init; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        [Required]
        public RateType RateType { get; init; }

        [Range(0, double.MaxValue)]
        public decimal RateValue { get; init; }

        public bool IsPublic { get; init; } = true;

        public bool IsActive { get; init; } = true;
    }
}
