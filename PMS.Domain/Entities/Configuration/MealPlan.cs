using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities.Configuration
{
    public class MealPlan
    {
		public int Id { get; set; }
		public string Name { get; set; } // BB, HB, FB

		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; } // سعر الوجبة الافتراضي

		public bool IsActive { get; set; } = true;
	}
}
