using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs
{
    public record RegisterDto
    {
        [Required]
        public string FullName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string UserName { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; init; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "Phone number must be 6–12 digits")]
        public string PhoneNumber { get; init; } = string.Empty;

        [Required]
        public string NationalId { get; init; } = string.Empty;

        public string? CountryID { get; init; }
    }
}
