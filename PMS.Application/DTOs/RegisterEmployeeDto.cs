using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs
{
    public record RegisterEmployeeDto
    {
        [Required]
        public string FullName { get; init; } = string.Empty;

        [Required]
        public string Username { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string PhoneNumber { get; init; } = string.Empty;

        public string? WorkNumber { get; init; }

        [Required]
        public string Password { get; init; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; init; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "National ID must be 10 digits")]
        public string NationalId { get; init; } = string.Empty;

        [Required]
        public string Nationality { get; init; } = string.Empty;

        public string? Gender { get; init; }

        [Required]
        public string Role { get; init; } = string.Empty;

        public DateTime? BirthdayDate { get; init; }

        public IFormFile? ProfileImage { get; init; }

        public List<IFormFile>? EmployeeDocs { get; init; }

        public bool IsActive { get; init; } = false;
        public bool ChangePasswordApprove { get; init; } = true;
    }
}
