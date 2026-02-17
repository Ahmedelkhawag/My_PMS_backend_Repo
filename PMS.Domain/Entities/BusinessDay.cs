using PMS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities
{
    /// <summary>
    /// Represents a financial/business day in the hotel.
    /// Only one BusinessDay can be Open at any given time.
    /// </summary>
    public class BusinessDay
    {
        public int Id { get; set; }

        /// <summary>
        /// The calendar date of this business day.
        /// Only the date part is meaningful; time is ignored.
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Current status of the business day (Open/Closed).
        /// </summary>
        [Required]
        public BusinessDayStatus Status { get; set; }

        /// <summary>
        /// When the business day was opened (UTC).
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the business day was closed (UTC). Null while open.
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// User who closed the business day.
        /// </summary>
        public string? ClosedById { get; set; }

        public virtual AppUser? ClosedBy { get; set; }
    }
}

