using System;

namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Summary view of a guest folio used for header/balance displays.
    /// </summary>
    public class GuestFolioSummaryDto
    {
        public int ReservationId { get; set; }
        public int FolioId { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public string Currency { get; set; } = "EGP";
    }
}

