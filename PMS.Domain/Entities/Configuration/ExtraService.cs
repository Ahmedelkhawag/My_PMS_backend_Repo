using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities.Configuration
{
    public class ExtraService
    {
		public int Id { get; set; }
		public string Name { get; set; } // "Spa", "Airport Pickup"
		public decimal Price { get; set; } // 300.00
		public bool IsPerDay { get; set; } // هل تحسب يومياً؟ (Parking = true, Transfer = false)
		public bool IsActive { get; set; } = true;
	}
}
