using Microsoft.AspNetCore.Identity;
using PMS.Domain.Enums;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace PMS.Domain.Entities
{
    public class AppUser : IdentityUser, ISoftDeletable
    {
        [Required]
        public override string UserName { get; set; }

        [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "Phone number must be 6–12 digits")]
        public override string? PhoneNumber { get; set; }
        public string? WorkNumber { get; set; }

        [RegularExpression(@"^\+[1-9]\d{0,3}$", ErrorMessage = "Invalid country code (e.g. +20, +1, +44)")]
        public string? CountryCode { get; set; }
        public Gender? Gender { get; set; }
        public string NationalId { get; set; }
        public string Nationality { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ChangePasswordApprove { get; set; } = true;
        public string? FullName { get; set; }
        public Guid? StatusID { get; set; }
        public virtual Status? Status { get; set; }
        public string? CountryID { get; set; }
        public virtual Country? Country { get; set; }
        public DateTime? DateOfBirth { get; set; } = DateTime.MinValue;
        public string? ProfileImagePath { get; set; }
        public int? HotelId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public virtual ICollection<EmployeeDocument> EmployeeDocs { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
