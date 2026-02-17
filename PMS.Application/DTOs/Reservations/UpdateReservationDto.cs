using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace PMS.Application.DTOs.Reservations
{
    public class UpdateReservationDto
    {
		[JsonIgnore]
		public int Id { get; set; } // بيتم تعيينه من الـ route في الكنترولر

		// البيانات الأساسية (nullable عشان الـ update يكون جزئي)
		public int? GuestId { get; set; }
		public int? RoomTypeId { get; set; }
		public int? RoomId { get; set; }

		public int? CompanyId { get; set; }

		// التواريخ (لو اتغيرت هيتغير السعر) - nullable لتحديث جزئي
		public DateTime? CheckInDate { get; set; }
		public DateTime? CheckOutDate { get; set; }

		// الماليات
		public decimal? NightlyRate { get; set; }
		public decimal? DiscountAmount { get; set; }

		// تفاصيل البيزنس
		public string? RateCode { get; set; }
		public int? MealPlanId { get; set; }      // بدل string MealPlan
		public int? BookingSourceId { get; set; } // بدل SourceId
		public int? MarketSegmentId { get; set; }
		public string? PurposeOfVisit { get; set; }
		public string? Notes { get; set; }

		// الإضافات الأخيرة (عشان التحديث يشمل كل حاجة)
		public string? ExternalReference { get; set; }
		public string? CarPlate { get; set; }

		// الأعداد
		public int? Adults { get; set; }
		public int? Children { get; set; }

		// Flags
		public bool? IsPostMaster { get; set; }
		public bool? IsGuestPay { get; set; }
		public bool? IsNoExtend { get; set; }
		public bool? IsConfidentialRate { get; set; }

		// قائمة الخدمات (ExtraServiceId + Quantity; backend looks up name/price)
		public List<CreateReservationServiceDto>? Services { get; set; }
	}
}
