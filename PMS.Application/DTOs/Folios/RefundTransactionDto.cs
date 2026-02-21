using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Folios
{
    public class RefundTransactionDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}
