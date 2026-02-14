using System;

namespace PMS.Application.DTOs.Common
{
    public abstract class BaseAuditableDto
    {
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
