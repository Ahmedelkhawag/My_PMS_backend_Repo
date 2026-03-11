using PMS.Domain.Enums.BackOffice;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.BackOffice.AR
{
    /// <summary>
    /// Tracks a commission owed to a Travel Agent (TA) or OTA for a specific reservation.
    /// Only RoomCharge transactions count toward EligibleRevenue.
    /// </summary>
    public class TACommissionRecord : BaseAuditableEntity
    {
        // ── Foreign Keys ─────────────────────────────────────────────────────

        /// <summary>FK to CompanyProfile (the TA/OTA company).</summary>
        public int CompanyId { get; set; }
        public virtual CompanyProfile Company { get; set; } = null!;

        /// <summary>FK to the Reservation that generated this commission.</summary>
        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; } = null!;

        // ── Financial Fields ─────────────────────────────────────────────────

        /// <summary>Sum of RoomCharge folio transactions (incidentals excluded).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal EligibleRevenue { get; set; }

        /// <summary>Commission rate (%) captured at the time of calculation — snapshot of CompanyProfile.CommissionRate.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionRate { get; set; }

        /// <summary>Calculated amount = EligibleRevenue * (CommissionRate / 100).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        // ── Lifecycle ────────────────────────────────────────────────────────

        public CommissionStatus Status { get; set; } = CommissionStatus.Draft;

        /// <summary>Populated when the commission is Approved and posted to the GL.</summary>
        public int? JournalEntryId { get; set; }
        public virtual JournalEntry? JournalEntry { get; set; }
    }
}
