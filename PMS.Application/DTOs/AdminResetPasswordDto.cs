using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs
{
    public class AdminResetPasswordDto
    {
        [Required(ErrorMessage = "New password is required.")]
        public string NewPassword { get; set; }
    }
}
