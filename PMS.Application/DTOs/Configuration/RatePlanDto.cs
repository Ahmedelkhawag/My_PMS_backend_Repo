using System;
using System.Collections.Generic;
using System.Text;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs.Configuration
{
    public class RatePlanDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public RateType RateType { get; set; }
        public decimal RateValue { get; set; }
        public bool IsPublic { get; set; }
        public bool IsActive { get; set; }
    }
}

