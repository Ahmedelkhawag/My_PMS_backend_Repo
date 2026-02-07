using Microsoft.AspNetCore.Http;

namespace PMS.Application.DTOs.Auth
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
