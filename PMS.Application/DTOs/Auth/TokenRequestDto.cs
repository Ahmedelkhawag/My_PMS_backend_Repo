using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Auth
{
    public record TokenRequestDto
    {
        [Required]
        public string Token { get; init; } = string.Empty;

        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }
}
