using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class ReservationListDto
    {
		public int Id { get; set; }
		public string ReservationNumber { get; set; } // BK-2026...

		// بيانات العميل
		public string GuestName { get; set; }
		public string GuestPhone { get; set; }

		// بيانات الغرفة
		public string RoomNumber { get; set; }
		public string RoomTypeName { get; set; }

		// التواريخ
		public string CheckInDate { get; set; } // هنرجعها String فورمات جاهز للعرض
		public string CheckOutDate { get; set; }
		public int Nights { get; set; }

		// المالي
		public decimal GrandTotal { get; set; }
		public string Status { get; set; } // Confirmed, Pending...
		public string StatusColor { get; set; } // لون الحالة (اختياري للفرونت)
		public string? Notes { get; set; }
	}
}
