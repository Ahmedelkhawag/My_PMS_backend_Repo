using System;

namespace PMS.Application.DTOs.Audit
{
    public record AuditStatusDto
    {
        public DateTime CurrentBusinessDate { get; set; }
        public bool IsOpen { get; set; }
        public DateTime OpenSince { get; set; }
    }
}
