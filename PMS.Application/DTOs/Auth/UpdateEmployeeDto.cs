using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Auth
{
    public record UpdateEmployeeDto
    {
        public string? FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? NationalId { get; init; }
        public string? WorkNumber { get; init; }
        public string? Nationality { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public IFormFile? ProfileImage { get; init; }
        public string? Role { get; init; }
        public bool? IsActive { get; init; }
        public List<IFormFile>? EmployeeDocs { get; init; }
    }
}
