using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities.Configuration
{
    public class BookingSource
    {
        public int Id { get; set; }
        public string Name { get; set; } // Booking.com, Expedia, Walk-in
        public bool IsActive { get; set; } = true;
        public bool RequiresExternalReference { get; set; } = false;
    }
}
