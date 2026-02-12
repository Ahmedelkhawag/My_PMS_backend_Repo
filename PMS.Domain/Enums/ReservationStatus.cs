using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
    /// <summary>
    /// حالة الحجز:
    /// 1 = Pending
    /// 2 = Confirmed
    /// 3 = CheckIn
    /// 4 = CheckOut
    /// 5 = Cancelled
    /// 6 = NoShow
    /// </summary>
    public enum ReservationStatus
    {
        Pending = 1,
        Confirmed = 2,
        CheckIn = 3,
        CheckOut = 4,
        Cancelled = 5,
        NoShow = 6
    }
}
