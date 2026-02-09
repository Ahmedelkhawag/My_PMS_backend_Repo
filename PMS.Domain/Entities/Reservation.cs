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

		// 1. الأساسيات
		[Required]
		public string ReservationNumber { get; set; } = string.Empty;
		public string? ExternalReference { get; set; }

		// 2. العلاقات (النزيل والغرفة)
		[Required]
		public int GuestId { get; set; }
		public Guest Guest { get; set; }

		public int? RoomId { get; set; }
		public Room? Room { get; set; }

		[Required]
		public int RoomTypeId { get; set; }
		public RoomType RoomType { get; set; }

		public ICollection<ReservationService> Services { get; set; } = new List<ReservationService>();

		// 3. التواريخ
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }

		// 4. الماليات
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

		// 5. الحالة والمصدر
		public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

		// 👇 تعديل المصدر (بدل Enum بقى جدول)
		public int BookingSourceId { get; set; }
		public BookingSource BookingSource { get; set; }


		// 6. تفاصيل البيزنس (Lookups)
		// ==========================================

		public string RateCode { get; set; } = "Standard"; // دي لسه معملنلهاش جدول فهنسيبها نص مؤقتاً

		// 👇 تعديل خطة الوجبات (بدل string بقى جدول)
		public int MealPlanId { get; set; }
		public MealPlan MealPlan { get; set; }

		// 👇 تعديل قطاع السوق (بدل string بقى جدول)
		public int MarketSegmentId { get; set; }
		public MarketSegment MarketSegment { get; set; }

		// تفاصيل أخرى
		public bool IsPostMaster { get; set; } = false;
		public bool IsNoExtend { get; set; } = false;
		public bool IsGuestPay { get; set; } = true;

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
