using PMS.Domain.Entities.Configuration;
using PMS.Domain.Enums;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{
    public class Reservation:ISoftDeletable, IAuditable
	{


		public int Id { get; set; }

		[Required]
		public string ReservationNumber { get; set; } = string.Empty;
		public string? ExternalReference { get; set; }

		
		[Required]
		public int GuestId { get; set; }
		public Guest Guest { get; set; }

		public int? RoomId { get; set; }
		public Room? Room { get; set; }

		[Required]
		public int RoomTypeId { get; set; }
		public RoomType RoomType { get; set; }

		public int? CompanyId { get; set; }
		public CompanyProfile? Company { get; set; }

		public ICollection<ReservationService> Services { get; set; } = new List<ReservationService>();

	
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }

		
		[Column(TypeName = "decimal(18,2)")]
		public decimal NightlyRate { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalAmount { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal ServicesAmount { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal TaxAmount { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal GrandTotal { get; set; }

		public ReservationStatus Status { get; set; } = ReservationStatus.Pending;


		public int BookingSourceId { get; set; }
		public BookingSource BookingSource { get; set; }



		public string RateCode { get; set; } = "Standard"; 

		
		public int MealPlanId { get; set; }
		public MealPlan MealPlan { get; set; }

	
		public int MarketSegmentId { get; set; }
		public MarketSegment MarketSegment { get; set; }

		public bool IsPostMaster { get; set; } = false;
		public bool IsNoExtend { get; set; } = false;
		public bool IsGuestPay { get; set; } = true;
		public bool IsConfidentialRate { get; set; } = false;

		public int Adults { get; set; } = 1;
		public int Children { get; set; } = 0;
		public string? Notes { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountAmount { get; set; } = 0;

		public string? PurposeOfVisit { get; set; }
		public string? CarPlate { get; set; }

		// Auditing Fields
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
	}
}
