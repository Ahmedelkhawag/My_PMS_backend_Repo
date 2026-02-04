using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs
{
    public class RegisterEmployeeDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string? WorkNumber { get; set; } // اختياري

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "National ID must be 10 digits")]
        public string NationalId { get; set; } // التزمنا بطلبك (10 أرقام)

        [Required]
        public string Nationality { get; set; }

        public string? Gender { get; set; } // اختياري

        [Required]
        public string Role { get; set; } // RECEPTIONIST, ACCOUNTANT, etc.

        public DateTime? BirthdayDate { get; set; }

        // --- التعامل مع الملفات ---

        public IFormFile? ProfileImage { get; set; } // صورة شخصية واحدة

        public List<IFormFile>? EmployeeDocs { get; set; } // لستة ملفات (فيش، شهادات، إلخ)

        public bool IsActive { get; set; } = false;
        public bool ChangePasswordApprove { get; set; } = true;
    }
}
