using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Reservations
{
    public class ReservationServiceDto
    {
		public string ServiceName { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public bool IsPerDay { get; set; }
		public decimal Total { get; set; }
		public int? ExtraServiceId { get; set; }
	}
}
