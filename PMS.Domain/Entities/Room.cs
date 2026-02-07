using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{

    public class Room
    {
        public int Id { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty; // رقم الغرفة (101, 102, A-01) - لازم يكون مميز

        public int FloorNumber { get; set; } // رقم الطابق (1, 2, 3...) زي ما ظاهر في الفلتر

        public RoomStatus Status { get; set; } = RoomStatus.Available; // الحالة الافتراضية "متاحة"

        public string? Notes { get; set; } // ملاحظات (مثلاً: "التكييف محتاج صيانة")

        public bool IsActive { get; set; } = true; // لو حبيت توقف الغرفة مؤقتاً من غير ما تمسحها

        // --- العلاقات (Relationships) ---

        // كل غرفة لازم يكون ليها نوع واحد (Single, Double, etc.)
        public int RoomTypeId { get; set; }

        [ForeignKey("RoomTypeId")]
        public RoomType? RoomType { get; set; }
    }
}

