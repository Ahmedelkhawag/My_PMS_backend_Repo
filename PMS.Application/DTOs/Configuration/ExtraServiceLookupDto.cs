using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Configuration
{
    public class ExtraServiceLookupDto
    {
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public bool IsPerDay { get; set; }
	}
}
