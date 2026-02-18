using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Folios
{
    public class PostPaymentWithDiscountDto
    {
        [Required]
        public int ReservationId { get; set; }

        // 👇 1. بيانات الدفع 👇
        [Required]
        public TransactionType PaymentType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal PaymentAmount { get; set; }

        [Required]
        [MaxLength(500)]
        public string PaymentDescription { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        // 👇 2. بيانات الخصم (اختيارية) 👇
        public bool ApplyDiscount { get; set; }

        public decimal? DiscountAmount { get; set; }

        [MaxLength(500)]
        public string? DiscountDescription { get; set; }

        [MaxLength(500)]
        public string? DiscountReason { get; set; }
    }
}
