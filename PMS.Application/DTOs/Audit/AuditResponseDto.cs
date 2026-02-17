using System;

namespace PMS.Application.DTOs.Audit
{
    public class AuditResponseDto
    {
        /// <summary>
        /// Number of rooms processed during the night audit.
        /// </summary>
        public int ProcessedRooms { get; set; }

        /// <summary>
        /// Number of reservations marked as no-show.
        /// </summary>
        public int NoShowsMarked { get; set; }

        /// <summary>
        /// The new business date after the audit completes.
        /// </summary>
        public DateTime NewBusinessDate { get; set; }

        /// <summary>
        /// Summary message describing the audit result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

