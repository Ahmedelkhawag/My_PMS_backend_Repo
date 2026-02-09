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

		// 2. العلاقات
		[Required]
		public int GuestId { get; set; }
		public Guest Guest { get; set; }

		public int? RoomId { get; set; }
		public Room? Room { get; set; }

		[Required]
		public int RoomTypeId { get; set; }
		public RoomType RoomType { get; set; }

		// 👇👇 الجديد: قائمة الخدمات الإضافية 👇👇
		public ICollection<ReservationService> Services { get; set; } = new List<ReservationService>();

		// 3. التواريخ
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }

		// 4. الماليات
		[Column(TypeName = "decimal(18,2)")]
		public decimal NightlyRate { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalAmount { get; set; } // سعر الغرفة * الليالي

		[Column(TypeName = "decimal(18,2)")]
		public decimal ServicesAmount { get; set; } // 👇 إجمالي الخدمات الإضافية

		[Column(TypeName = "decimal(18,2)")]
		public decimal TaxAmount { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal GrandTotal { get; set; } // TotalAmount + ServicesAmount + Tax

		// 5. الحالة
		public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
		public ReservationSource Source { get; set; } = ReservationSource.Direct;

		// 6. تفاصيل البيزنس والفوترة (تمت إضافتها) ✅
		// ==========================================
		public string RateCode { get; set; } = "Standard"; // "Corporate", "Gov"
		public string MealPlan { get; set; } = "RoomOnly";

		// Billing Instructions (Checkboxes)
		public bool IsPostMaster { get; set; } = false;
		public bool IsNoExtend { get; set; } = false;
		public bool IsGuestPay { get; set; } = true; // Guest Pay Services

		// 7. أخرى
		public int Adults { get; set; } = 1;
		public int Children { get; set; } = 0;
		public string? Notes { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountAmount { get; set; } = 0;

		// الغرض من الزيارة (عمل، سياحة، علاج..) - مهم للتقارير الأمنية
		public string? PurposeOfVisit { get; set; }

		// القطاع التسويقي (شركات، أفراد، مجموعات..) - مهم للإحصائيات
		public string? MarketSegment { get; set; }

		

		// رقم السيارة (لأغراض الأمن) - يقابل CarPlat
		public string? CarPlate { get; set; }

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
		public DateTime CreatedAt { get; set; } 
		public string? CreatedBy { get; set; }
		

		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
	}
}
