namespace PMS.Application.DTOs.Folios
{
    /// <summary>
    /// Summary view of a guest folio used for header/balance displays.
    /// </summary>
    public record GuestFolioSummaryDto
    {
        public int ReservationId { get; init; }
        public int FolioId { get; init; }
        public decimal TotalCharges { get; init; }
        public decimal TotalPayments { get; init; }
        public decimal Balance { get; init; }
        public bool IsActive { get; init; }
        public string Currency { get; init; } = "EGP";
    }
}
