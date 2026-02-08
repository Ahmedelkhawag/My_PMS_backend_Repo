using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Guests
{
    public class GuestDto
    {
		public int Id { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string NationalId { get; set; }
		public string Nationality { get; set; }
		public string LoyaltyLevel { get; set; } 
		public DateTime? DateOfBirth { get; set; }
		public string? Email { get; set; }
		public string? CarNumber { get; set; }
		public string? Notes { get; set; }
		public bool IsActive { get; set; }
	}
}
