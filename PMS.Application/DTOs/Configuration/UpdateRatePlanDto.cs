using System.ComponentModel.DataAnnotations;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Configuration
{
    public record UpdateRatePlanDto
    {
        [MaxLength(50)]
        public string? Code { get; init; }

        [MaxLength(200)]
        public string? Name { get; init; }

        public string? Description { get; init; }

        public RateType? RateType { get; init; }

        [Range(0, double.MaxValue)]
        public decimal? RateValue { get; init; }

        public bool? IsPublic { get; init; }

        public bool? IsActive { get; init; }
    }
}
