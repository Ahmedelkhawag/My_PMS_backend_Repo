using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs
{
    public record AdminResetPasswordDto
    {
        [Required(ErrorMessage = "New password is required.")]
        public string NewPassword { get; init; } = string.Empty;
    }
}
