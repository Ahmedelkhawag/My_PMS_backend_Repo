using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
    public enum RoomStatus
    {
        Available = 0,   // متاح (أخضر)
        Occupied = 1,    // مشغول (أحمر)
        Maintenance = 2, // صيانة (أصفر)
        Cleaning = 3,    // تنظيف (بنفسجي)
        Reserved = 4     // محجوزة (لسه النزيل موصلش - لون أزرق مثلاً) - اختياري
    }
}
