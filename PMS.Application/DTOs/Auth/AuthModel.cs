using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.Auth
{
    public record AuthModel
    {
        public string? Message { get; set; }
        public bool IsAuthenticated { get; init; }
        public string? Username { get; init; }
        public string? Email { get; init; }
        public bool ChangePasswordApprove { get; init; }
        public int? HotelId { get; init; }
        public List<string>? Roles { get; init; }
        public string? Token { get; init; }
        public DateTime ExpiresOn { get; init; }
        public string? RefreshToken { get; init; }
        public DateTime RefreshTokenExpiration { get; init; }
    }
}
