using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class ReservationDto
    {
		public int Id { get; set; }
		public string ReservationNumber { get; set; }

		// 1. بيانات النزيل (تفصيلية) 🆕
		public int GuestId { get; set; }
		public string GuestName { get; set; }
		public string GuestPhone { get; set; }  // إضافة
		public string? GuestEmail { get; set; } // إضافة
		public string? GuestNationalId { get; set; } // إضافة

		// 2. بيانات الغرفة
		public int RoomTypeId { get; set; }
		public string RoomTypeName { get; set; }
		public int? RoomId { get; set; }
		public string? RoomNumber { get; set; }

		// 3. التواريخ
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int Nights { get; set; }

		// 4. تفاصيل البيزنس (مهمة للعرض) 🆕
		public string RateCode { get; set; }  // إضافة (Standard, Corporate)
		public string MealPlan { get; set; }  // إضافة (Breakfast..)
		public string Source { get; set; }    // إضافة (Booking.com..)

		// 5. الماليات
		public decimal NightlyRate { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal ServicesAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal GrandTotal { get; set; }

		public string Status { get; set; }
		public string? Notes { get; set; }

		public string? ExternalReference { get; set; }
		public string? CarPlate { get; set; }
		public string? PurposeOfVisit { get; set; }
		public string? MarketSegment { get; set; }

		// 6. الخدمات
		public List<ReservationServiceDto> Services { get; set; }
	}
}
