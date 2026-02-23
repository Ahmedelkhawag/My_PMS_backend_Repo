using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class ChangeReservationStatusDto
    {
		[Required]
		public int ReservationId { get; set; }

		/// <summary>
		/// حالة الحجز الجديدة (1 = Pending, 2 = Confirmed, 3 = CheckIn, 4 = CheckOut, 5 = Cancelled, 6 = NoShow).
		/// </summary>
		[Required]
		public ReservationStatus NewStatus { get; set; } // (CheckedIn, Cancelled, CheckedOut)

		// اختياري: لو بيعمل Check-In ولسه مختارش غرفة، لازم يبعتها هنا
		public int? RoomId { get; set; }

		public string? Note { get; set; } // سبب الإلغاء مثلاً

		public decimal? FeeAmount { get; set; }

		public string? FeeReason { get; set; }
	}
}
