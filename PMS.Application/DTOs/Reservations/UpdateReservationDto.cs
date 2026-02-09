using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class UpdateReservationDto
    {
		[Required]
		public int Id { get; set; } // لازم نعرف بنعدل انهي حجز

		// البيانات الأساسية
		public int GuestId { get; set; }
		public int RoomTypeId { get; set; }
		public int? RoomId { get; set; }

		// التواريخ (لو اتغيرت هيتغير السعر)
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }

		// الماليات
		public decimal NightlyRate { get; set; }
		public decimal DiscountAmount { get; set; } = 0;

		// تفاصيل البيزنس
		public string? RateCode { get; set; }
		public int MealPlanId { get; set; }      // بدل string MealPlan
		public int BookingSourceId { get; set; } // بدل SourceId
		public int MarketSegmentId { get; set; }
		public string? PurposeOfVisit { get; set; }
		public string? Notes { get; set; }

		// الإضافات الأخيرة (عشان التحديث يشمل كل حاجة)
		public string? ExternalReference { get; set; }
		public string? CarPlate { get; set; }

		// الأعداد
		public int Adults { get; set; }
		public int Children { get; set; }

		// Flags
		public bool IsPostMaster { get; set; }
		public bool IsGuestPay { get; set; }
		public bool IsNoExtend { get; set; }

		// قائمة الخدمات (ممكن النزيل يزود خدمة أو يلغي خدمة)
		public List<ReservationServiceDto>? Services { get; set; }
	}
}
