using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Auth
{
    public class UpdateEmployeeDto
    {
        [Required]
        public string Id { get; set; } // لازم نعرف بنعدل مين

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NationalId { get; set; }
        public string? WorkNumber { get; set; }
        public string? Nationality { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // لو بعت صورة، هنغير القديمة. لو مبعتش، هنسيب القديمة.
        public IFormFile? ProfileImage { get; set; }

        // لو عايز يغير الرول (للمديرين فقط)
        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        public List<IFormFile>? EmployeeDocs { get; set; }
    }
}
