using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Shifts
{
    public record CloseShiftDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Actual cash must be zero or greater.")]
        public decimal ActualCash { get; init; }

        [MaxLength(2000)]
        public string Notes { get; init; } = string.Empty;
    }
}
