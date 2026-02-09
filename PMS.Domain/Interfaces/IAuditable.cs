using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Interfaces
{
    public interface IAuditable
    {
		string? CreatedBy { get; set; }
		DateTime CreatedAt { get; set; }

		// مين اللي عدل (Updated By)
		string? LastModifiedBy { get; set; }
		DateTime? LastModifiedAt { get; set; }
	}
}
