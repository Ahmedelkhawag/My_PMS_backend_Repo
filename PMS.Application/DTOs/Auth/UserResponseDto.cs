using System;
using PMS.Application.DTOs.Common;

namespace PMS.Application.DTOs.Auth
{
    public class UserResponseDto : BaseAuditableDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; } // true = Active, false = Inactive/Suspended
        public string Role { get; set; } // Manager, Receptionist...
        public int? HotelId { get; set; }
    }
}
