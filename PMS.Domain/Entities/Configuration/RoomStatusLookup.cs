using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities.Configuration
{
    public class RoomStatusLookup
    {
		public int Id { get; set; }
		public string Name { get; set; } // Clean, Dirty, Maintenance, Out of Order
		public string Color { get; set; } // Hex Code (#FFFFFF) عشان الفرونت يلونها
		public bool IsActive { get; set; } = true;
	}
}
