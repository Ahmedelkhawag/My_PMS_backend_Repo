using System;
using PMS.Application.DTOs.Common;

namespace PMS.Application.DTOs.Guests
{
    public record GuestDto : BaseAuditableDto
    {
        public int Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? NationalId { get; init; }
        public string? Nationality { get; init; }
        public string LoyaltyLevel { get; init; } = string.Empty;
        public DateTime? DateOfBirth { get; init; }
        public string? Email { get; init; }
        public string? CarNumber { get; init; }
        public string? Notes { get; init; }
        public bool IsActive { get; init; }
    }
}
