using Microsoft.AspNetCore.Http;

namespace PMS.Application.DTOs.Auth
{
    public record UpdateProfileDto
    {
        public string? FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public IFormFile? ProfileImage { get; init; }
    }
}
