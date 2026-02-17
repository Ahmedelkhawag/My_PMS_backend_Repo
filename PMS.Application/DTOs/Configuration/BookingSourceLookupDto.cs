using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Configuration
{
    public class BookingSourceLookupDto : LookupDto
    {
        public bool RequiresExternalReference { get; set; }
    }
}

