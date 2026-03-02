using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{
    public class ReservationService:ISoftDeletable, IAuditable
	{
		public int Id { get; set; }

	
		public int ReservationId { get; set; }
		[ForeignKey("ReservationId")]
		public Reservation Reservation { get; set; } 

	
		[Required]
		public string ServiceName { get; set; } = string.Empty; // "Spa", "Airport Pickup"

		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; } 

		public int Quantity { get; set; } = 1; 

		public bool IsPerDay { get; set; } = false;

		
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalServicePrice { get; set; }

		public string? CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }

		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
	}
}
