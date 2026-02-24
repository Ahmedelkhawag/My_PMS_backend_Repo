using PMS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Guests
{
    /// <summary>
    /// Partial update: send only the fields you want to change. Omitted fields are left unchanged.
    /// </summary>
    public record UpdateGuestDto
    {
        [MaxLength(200)]
        public string? FullName { get; init; }

        [MaxLength(20)]
        public string? PhoneNumber { get; init; }

        [MaxLength(50)]
        public string? NationalId { get; init; }

        [MaxLength(100)]
        public string? Nationality { get; init; }

        public DateTime? DateOfBirth { get; init; }

        [MaxLength(100)]
        public string? Email { get; init; }

        [MaxLength(500)]
        public string? Address { get; init; }

        [MaxLength(100)]
        public string? City { get; init; }

        [MaxLength(20, ErrorMessage = "رقم السيارة لا يجب أن يتجاوز 20 حرفاً")]
        public string? CarNumber { get; init; }

        [MaxLength(50, ErrorMessage = "الرقم الضريبي لا يجب أن يتجاوز 50 حرفاً")]
        public string? VatNumber { get; init; }

        public string? Notes { get; init; }

        /// <summary>Loyalty level (0 = Bronze, 1 = Silver, 2 = Gold, 3 = Platinum).</summary>
        public LoyaltyLevel? LoyaltyLevel { get; init; }
    }
}
