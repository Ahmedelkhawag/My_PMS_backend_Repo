using PMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Folios
{
    public record PostPaymentWithDiscountDto
    {
        [Required]
        public int ReservationId { get; init; }

        [Required]
        public TransactionType PaymentType { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal PaymentAmount { get; init; }

        [Required]
        [MaxLength(500)]
        public string PaymentDescription { get; init; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenceNo { get; init; }

        public bool ApplyDiscount { get; init; }

        public decimal? DiscountAmount { get; init; }

        [MaxLength(500)]
        public string? DiscountDescription { get; init; }

        [MaxLength(500)]
        public string? DiscountReason { get; init; }
    }
}
