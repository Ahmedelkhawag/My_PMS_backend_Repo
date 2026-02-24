using PMS.Application.DTOs.Common;
using System;

namespace PMS.Application.DTOs.Shifts
{
    public record ShiftDto : BaseAuditableDto
    {
        public int Id { get; init; }
        public string EmployeeId { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime? EndedAt { get; init; }
        public decimal StartingCash { get; init; }
        public decimal SystemCalculatedCash { get; init; }
        public decimal? ActualCashHanded { get; init; }
        public decimal? Difference { get; init; }
        public string Notes { get; init; } = string.Empty;
        public bool IsClosed { get; init; }
    }
}
