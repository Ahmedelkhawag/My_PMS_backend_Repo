using System;

namespace PMS.Application.DTOs.Reports
{
    public record PoliceReportDto
    {
        public string GuestName { get; init; } = string.Empty;
        public string Nationality { get; init; } = string.Empty;
        public string DocumentType { get; init; } = string.Empty;
        public string DocumentNumber { get; init; } = string.Empty;
        public string RoomNumber { get; init; } = string.Empty;
        public DateTime ArrivalDate { get; init; }
        public DateTime DepartureDate { get; init; }
        public string Profession { get; init; } = string.Empty;
    }
}
