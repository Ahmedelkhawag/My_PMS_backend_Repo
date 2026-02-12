using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
    /// <summary>
    /// حالة الغرفة:
    /// 0 = Available
    /// 1 = Occupied
    /// 2 = Maintenance
    /// 3 = Cleaning
    /// 4 = Reserved
    /// </summary>
    public enum RoomStatus
    {
        Available = 0,
        Occupied = 1,
        Maintenance = 2,
        Cleaning = 3,
        Reserved = 4
    }
}
