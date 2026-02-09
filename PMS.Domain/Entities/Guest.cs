using PMS.Domain.Enums;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Domain.Entities
{
	public class Guest : ISoftDeletable, IAuditable
	{
		public int Id { get; set; }

		[Required]
		public string FullName { get; set; } = string.Empty;

		[Required]
		[Phone]
		public string PhoneNumber { get; set; } = string.Empty;

		[Required]
		public string NationalId { get; set; } = string.Empty;

		public string Nationality { get; set; } = string.Empty;

		public DateTime? DateOfBirth { get; set; }

		public LoyaltyLevel LoyaltyLevel { get; set; } = LoyaltyLevel.Bronze;

		public string? Notes { get; set; }

		[EmailAddress]
		[MaxLength(100)]
		public string? Email { get; set; }

		public string? Address { get; set; }
		public string? City { get; set; }


		[MaxLength(20)]
		public string? CarNumber { get; set; }

		[MaxLength(50)]
		public string? VatNumber { get; set; }

		public bool IsActive { get; set; } = true;


		public string? CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }

		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
		
	}
}
