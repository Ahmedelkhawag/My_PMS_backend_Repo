using PMS.Application.DTOs.Common;

namespace PMS.Application.DTOs.Auth
{
    public record UserResponseDto : BaseAuditableDto
    {
        public string Id { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public string Role { get; set; } = string.Empty;
        public int? HotelId { get; init; }
    }
}
