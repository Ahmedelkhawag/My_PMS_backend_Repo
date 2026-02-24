using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs
{
    public record ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; init; } = string.Empty;

        [Required]
        public string NewPassword { get; init; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
