using PMS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Input DTO for adding a new transaction to a guest folio.
    /// Amount must always be positive; the business logic will apply
    /// the correct sign based on the transaction type (debit/credit).
    /// </summary>
    public class CreateTransactionDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        /// <summary>
        /// Positive transaction amount. The service layer will decide
        /// whether it increases charges or payments based on Type.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenceNo { get; set; }
    }
}

