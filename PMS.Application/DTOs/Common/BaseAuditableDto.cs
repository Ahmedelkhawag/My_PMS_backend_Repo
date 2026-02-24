using System;

namespace PMS.Application.DTOs.Common
{
    public abstract record BaseAuditableDto
    {
        public string? CreatedBy { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? UpdatedBy { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
