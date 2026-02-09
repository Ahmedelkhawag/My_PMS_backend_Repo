using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
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
