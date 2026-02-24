using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs
{
    public record LoginDto
    {
        [Required]
        public string UserName { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }
}
