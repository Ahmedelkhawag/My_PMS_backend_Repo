using System.Collections.Generic;

namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Detailed view of a guest folio including header summary and full transaction ledger.
    /// </summary>
    public class FolioDetailsDto
    {
        public int ReservationId { get; set; }
        public int FolioId { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public string Currency { get; set; } = "EGP";

        public List<FolioTransactionDto> Transactions { get; set; } = new List<FolioTransactionDto>();
    }
}

