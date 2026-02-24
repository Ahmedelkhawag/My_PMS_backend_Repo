using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Folios
{
    public record RefundTransactionDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required]
        public string Reason { get; init; } = string.Empty;
    }
}
