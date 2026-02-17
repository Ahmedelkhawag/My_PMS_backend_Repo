using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Configuration
{
    public class UpdateRatePlanDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public RateType RateType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RateValue { get; set; }

        public bool IsPublic { get; set; }

        public bool IsActive { get; set; }
    }
}

