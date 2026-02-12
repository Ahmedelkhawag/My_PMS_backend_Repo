using PMS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Guests
{
    /// <summary>
    /// Partial update: send only the fields you want to change. Omitted fields are left unchanged.
    /// </summary>
    public class UpdateGuestDto
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? NationalId { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20, ErrorMessage = "رقم السيارة لا يجب أن يتجاوز 20 حرفاً")]
        public string? CarNumber { get; set; }

        [MaxLength(50, ErrorMessage = "الرقم الضريبي لا يجب أن يتجاوز 50 حرفاً")]
        public string? VatNumber { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// مستوى الولاء (0 = Bronze, 1 = Silver, 2 = Gold, 3 = Platinum).
        /// </summary>
        public LoyaltyLevel? LoyaltyLevel { get; set; }
    }
}
