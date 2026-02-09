using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class CreateReservationServiceDto
    {
		[Required(ErrorMessage = "اسم الخدمة مطلوب")]
		public string ServiceName { get; set; } // "Spa", "VIP"

		[Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون قيمة موجبة")]
		public decimal Price { get; set; } // سعر الوحدة

		[Range(1, 100, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
		public int Quantity { get; set; } = 1;

		public bool IsPerDay { get; set; } // هل تضرب في عدد الليالي
	}
}
