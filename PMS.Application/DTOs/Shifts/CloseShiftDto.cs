using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Shifts
{
	public class CloseShiftDto
	{
		[Range(0, double.MaxValue, ErrorMessage = "Actual cash must be zero or greater.")]
		public decimal ActualCash { get; set; }

		[MaxLength(2000)]
		public string Notes { get; set; } = string.Empty;
	}
}

