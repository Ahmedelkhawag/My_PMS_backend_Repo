using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Configuration
{
    public class UpdateRatePlanDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public RateType? RateType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? RateValue { get; set; }

        public bool? IsPublic { get; set; }

        public bool? IsActive { get; set; }
    }
}

