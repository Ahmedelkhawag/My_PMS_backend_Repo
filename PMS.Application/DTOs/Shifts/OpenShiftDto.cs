using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Shifts
{
	public class OpenShiftDto
	{
		[Range(0, double.MaxValue, ErrorMessage = "Starting cash must be zero or greater.")]
		public decimal StartingCash { get; set; }
	}
}

