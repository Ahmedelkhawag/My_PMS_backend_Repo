using System;

namespace PMS.Application.DTOs.Shifts
{
	public class ShiftReportDto
	{
		public DateTime StartTime { get; set; }

		public decimal TotalCashPayments { get; set; }

		public decimal TotalCashRefunds { get; set; }

		public decimal TotalVisaPayments { get; set; }

		public decimal TotalVisaRefunds { get; set; }

		public decimal NetCash { get; set; }
	}
}

