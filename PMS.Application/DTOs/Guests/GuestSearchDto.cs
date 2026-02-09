using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Guests
{
    public class GuestSearchDto
    {
		public int Id { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string NationalId { get; set; }
	}
}
