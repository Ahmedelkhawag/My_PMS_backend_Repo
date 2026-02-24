using PMS.Application.DTOs.Reservations;
using System.Collections.Generic;

namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Detailed view of a guest folio including header summary and full transaction ledger.
    /// </summary>
    public record FolioDetailsDto
    {
        public int ReservationId { get; init; }
        public int FolioId { get; init; }
        public decimal TotalCharges { get; init; }
        public decimal TotalPayments { get; init; }
        public decimal Balance { get; init; }

        // The current actual balance based on posted transactions
        public decimal CurrentBalance => Balance;

        // The expected remaining balance until check-out
        public decimal ExpectedRemainingBalance { get; init; }

        public bool IsActive { get; init; }
        public string Currency { get; init; } = "EGP";

        // Full reservation details
        public ReservationDto? ReservationDetails { get; init; }

        public List<FolioTransactionDto> Transactions { get; init; } = new();
    }
}
