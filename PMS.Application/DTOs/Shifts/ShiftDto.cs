using PMS.Application.DTOs.Common;
using System;

namespace PMS.Application.DTOs.Shifts
{
	public class ShiftDto : BaseAuditableDto
	{
		public int Id { get; set; }

		public string EmployeeId { get; set; } = string.Empty;

		public DateTime StartedAt { get; set; }

		public DateTime? EndedAt { get; set; }

		public decimal StartingCash { get; set; }

		public decimal SystemCalculatedCash { get; set; }

		public decimal? ActualCashHanded { get; set; }

		public decimal? Difference { get; set; }

		public string Notes { get; set; } = string.Empty;

		public bool IsClosed { get; set; }
	}
}

