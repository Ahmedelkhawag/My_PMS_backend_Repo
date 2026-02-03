using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "Phone number must be 6–12 digits")]
        public string PhoneNumber { get; set; }

        [Required]
        public string NationalId { get; set; }

        public string? CountryID { get; set; }
    }
}
