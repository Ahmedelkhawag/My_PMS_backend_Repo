using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Configuration
{
    public class RatePlanLookupDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }
}

