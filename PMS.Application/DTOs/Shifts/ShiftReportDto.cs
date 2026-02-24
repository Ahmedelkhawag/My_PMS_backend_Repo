using System;

namespace PMS.Application.DTOs.Shifts
{
    public record ShiftReportDto
    {
        public DateTime StartTime { get; init; }
        public decimal TotalCashPayments { get; init; }
        public decimal TotalCashRefunds { get; init; }
        public decimal TotalVisaPayments { get; init; }
        public decimal TotalVisaRefunds { get; init; }
        public decimal NetCash { get; init; }
    }
}
