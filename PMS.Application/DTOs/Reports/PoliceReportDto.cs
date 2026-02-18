using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Reports
{
    public class PoliceReportDto
    {
        public string GuestName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;

        // National ID or Passport
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;

        public string RoomNumber { get; set; } = string.Empty;

        // Dates as DateTime to allow Excel formatting if needed, 
        // or we can format them as strings later.
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }

        public string Profession { get; set; } = string.Empty;
    }
}
