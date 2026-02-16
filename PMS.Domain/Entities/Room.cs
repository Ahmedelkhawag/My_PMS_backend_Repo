using PMS.Domain.Entities.Configuration;
using PMS.Domain.Enums;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{

    public class Room : ISoftDeletable, IAuditable
	{
        public int Id { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty; // رقم الغرفة (101, 102, A-01) - لازم يكون مميز

        public int FloorNumber { get; set; } // رقم الطابق (1, 2, 3...) زي ما ظاهر في الفلتر

        public string? Notes { get; set; } // ملاحظات (مثلاً: "التكييف محتاج صيانة")

        public bool IsActive { get; set; } = true; // لو حبيت توقف الغرفة مؤقتاً من غير ما تمسحها

        // --- العلاقات (Relationships) ---

        // كل غرفة لازم يكون ليها نوع واحد (Single, Double, etc.)
        public int RoomTypeId { get; set; }

        [ForeignKey("RoomTypeId")]
        public RoomType? RoomType { get; set; }

        public HKStatus HKStatus { get; set; } = HKStatus.Dirty;
        public BedType BedType { get; set; } = BedType.Single;
        public string? ViewType { get; set; }
        public int MaxAdults { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public FOStatus FOStatus { get; set; } = FOStatus.Vacant;

		public int RoomStatusId { get; set; } // بدل Enum - retained for legacy; FO status computed from reservations
		[ForeignKey("RoomStatusId")]
		public RoomStatusLookup RoomStatus { get; set; }

		// Maintenance (OOO) tracking
		public string? MaintenanceReason { get; set; }
		public DateTime? MaintenanceStartDate { get; set; }
		public DateTime? MaintenanceEndDate { get; set; }
		public string? MaintenanceRemarks { get; set; }
		public string? CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }

		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }
	}
}

