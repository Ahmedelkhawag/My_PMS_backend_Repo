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
		public string MealPlan { get; set; } = "RoomOnly"; // خطة الوجبات

		// خيارات الفوترة (Checkboxes)
		public bool IsPostMaster { get; set; }
		public bool IsGuestPay { get; set; }
		public bool IsNoExtend { get; set; }

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

		// مصدر الحجز (موبايل، بوكينج، الخ)
		public int SourceId { get; set; } = 1; // Direct by default

		public decimal DiscountAmount { get; set; } = 0;
		public string? PurposeOfVisit { get; set; }
		public string? MarketSegment { get; set; }

		public string? ExternalReference { get; set; }
		public string? CarPlate { get; set; }
	}
}
