using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{
    public class ReservationService:ISoftDeletable, IAuditable
	{
		public int Id { get; set; }

		// ==========================
		// 1. الربط بالحجز
		// ==========================
		public int ReservationId { get; set; }
		[ForeignKey("ReservationId")]
		public Reservation Reservation { get; set; } // عشان نرجع للحجز الأب

		// ==========================
		// 2. تفاصيل الخدمة
		// ==========================
		// هنخزن الاسم والسعر هنا عشان لو سعر الخدمة اتغير في المستقبل، الحجز القديم يفضل بسعره القديم (Snapshot)
		[Required]
		public string ServiceName { get; set; } = string.Empty; // "Spa", "Airport Pickup"

		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; } // 300.00 (سعر الوحدة)

		public int Quantity { get; set; } = 1; // العدد (لو طلب خدمتين غسيل مثلاً)

		public bool IsPerDay { get; set; } = false; // هل الخدمة دي بتتحسب يومياً؟ (زي VIP في الصورة)

		// ==========================
		// 3. الإجمالي الخاص بالخدمة دي
		// ==========================
		// لو هي يومية = السعر * العدد * عدد الليالي
		// لو مرة واحدة = السعر * العدد
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalServicePrice { get; set; }

		public string? CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }

		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
	}
}
