using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
    /// <summary>
    /// مصدر الحجز:
    /// 1 = Direct
    /// 2 = Website
    /// 3 = BookingCom
    /// 4 = Expedia
    /// 5 = Phone
    /// 6 = Corporate
    /// </summary>
    public enum ReservationSource
    {
        Direct = 1,
        Website = 2,
        BookingCom = 3,
        Expedia = 4,
        Phone = 5,
        Corporate = 6
    }
}
