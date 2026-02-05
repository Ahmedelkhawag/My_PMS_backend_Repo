using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Auth
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; } // true = Active, false = Inactive/Suspended
        public string Status { get; set; } // Derived from IsActive for display (\"Active\" / \"Inactive\")
        public string Role { get; set; } // Manager, Receptionist...
        public DateTime CreatedAt { get; set; } // لو عندك، أو ممكن نستغنى عنها
        public int? HotelId { get; set; }
    }
}
