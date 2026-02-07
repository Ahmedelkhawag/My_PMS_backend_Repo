using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{
    public class RoomType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // مثال: فردية، مزدوجة، جناح ملكي

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; } // السعر المبدئي لليلة (زي ما ظاهر في الكارت 540 ر.س)

        public string? Description { get; set; } // وصف (تطل على البحر، بها جاكوزي...)

        public int MaxAdults { get; set; } // أقصى عدد بالغين
        public int MaxChildren { get; set; } // أقصى عدد أطفال

        // علاقة: النوع الواحد ممكن يكون عليه غرف كتير
        // (هنفك الكومنت ده الخطوة الجاية لما نعمل كلاس الغرفة)
        public ICollection<Room> Rooms { get; set; }
    }
}
