using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities.Configuration
{
	public class MarketSegment
    {
		public int Id { get; set; }
		public string Name { get; set; } // Corporate, Individual, Government
		public bool IsActive { get; set; } = true;
	}
}
