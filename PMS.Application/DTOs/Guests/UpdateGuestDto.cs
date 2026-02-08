using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Guests
{
    public class UpdateGuestDto:CreateGuestDto
    {
		[Required]
		public int Id { get; set; }

		
		public LoyaltyLevel LoyaltyLevel { get; set; }
	}
}
