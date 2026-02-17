using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class CreateReservationDto
    {
		[Required(ErrorMessage = "النزيل مطلوب")]
		public int GuestId { get; set; }

		[Required(ErrorMessage = "نوع الغرفة مطلوب")]
		public int RoomTypeId { get; set; }

		// ممكن يكون null لو حجز "Waiting List" أو لسه مخصصناش غرفة
		public int? RoomId { get; set; }

		public int? CompanyId { get; set; }

		public int? RatePlanId { get; set; }

		// ==========================
		// 2. التواريخ
		// ==========================
		[Required]
		public DateTime CheckInDate { get; set; }

		[Required]
		public DateTime CheckOutDate { get; set; }

		// ==========================
		// 3. التفاصيل المالية (من صورة image_ee8e28)
		// ==========================
		[Required(ErrorMessage = "سعر الليلة مطلوب")]
		public decimal NightlyRate { get; set; } // السعر المتفق عليه

		public string RateCode { get; set; } = "Standard"; // كود السعر

		/// <summary>
		/// Allows overriding the calculated rate from the selected Rate Plan.
		/// </summary>
		public bool IsRateOverridden { get; set; } = false;
		[Required(ErrorMessage = "خطة الوجبات مطلوبة")]
		public int MealPlanId { get; set; }

		// خيارات الفوترة (Checkboxes)
		public bool IsPostMaster { get; set; }
		public bool IsGuestPay { get; set; }
		public bool IsNoExtend { get; set; }
		public bool IsConfidentialRate { get; set; } = false;

		// Walk-in: immediate check-in and room occupancy
		public bool IsWalkIn { get; set; } = false;

		// ==========================
		// 4. الخدمات الإضافية (القائمة)
		// ==========================
		// هنا بنستقبل قائمة من الخدمات اللي عرفناها فوق
		public List<CreateReservationServiceDto>? Services { get; set; }

		// ==========================
		// 5. أخرى
		// ==========================
		public int Adults { get; set; } = 1;
		public int Children { get; set; } = 0;
		public string? Notes { get; set; }

		public decimal DiscountAmount { get; set; } = 0;
		public string? PurposeOfVisit { get; set; }

		// مصدر الحجز (Lookup على جدول BookingSources)
		public int BookingSourceId { get; set; }

		// 👇 التعديل هنا: MarketSegment بقت ID
		public int MarketSegmentId { get; set; }

		public string? ExternalReference { get; set; }
		public string? CarPlate { get; set; }
	}
}
