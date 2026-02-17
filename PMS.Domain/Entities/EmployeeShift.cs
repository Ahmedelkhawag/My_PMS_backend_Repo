using PMS.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities
{
	public class EmployeeShift : ISoftDeletable, IAuditable
	{
		public int Id { get; set; }

		[Required]
		public string EmployeeId { get; set; } = string.Empty;

		public virtual AppUser? Employee { get; set; }

		/// <summary>
		/// Shift start time (UTC).
		/// </summary>
		public DateTime StartedAt { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Shift end time (UTC). Null means the shift is still open.
		/// </summary>
		public DateTime? EndedAt { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal StartingCash { get; set; }

		/// <summary>
		/// Total cash processed by the system during this shift.
		/// </summary>
		[Column(TypeName = "decimal(18,2)")]
		public decimal SystemCalculatedCash { get; set; }

		/// <summary>
		/// Cash amount handed at closing (entered by user).
		/// </summary>
		[Column(TypeName = "decimal(18,2)")]
		public decimal? ActualCashHanded { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal? Difference { get; set; }

		public string Notes { get; set; } = string.Empty;

		public bool IsClosed { get; set; } = false;

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		public string? CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? LastModifiedBy { get; set; }
		public DateTime? LastModifiedAt { get; set; }
	}
}

