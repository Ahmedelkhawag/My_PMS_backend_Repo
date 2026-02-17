using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs
{
    public class UserFilterDto
    {
        public int PageNumber { get; set; } = 1;      // الافتراضي الصفحة الأولى
        public int PageSize { get; set; } = 20;       // الافتراضي 10 موظفين
        public string? Search { get; set; }

        public string? Role { get; set; }   // فلتر بالرول (مثلاً: "Manager")
        public bool? IsActive { get; set; } // فلتر بالحالة المنطقية: true/false

		/// <summary>
		/// When true, returns only closed items that have a non-zero discrepancy (Difference).
		/// Used by shift history reporting.
		/// </summary>
		public bool? ShowOnlyDiscrepancies { get; set; }
    }
}
