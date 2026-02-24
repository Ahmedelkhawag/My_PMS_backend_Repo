namespace PMS.Application.DTOs.Reservations
{
    public record ReservationServiceDto
    {
        public string ServiceName { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public int Quantity { get; init; }
        public bool IsPerDay { get; init; }
        public decimal Total { get; init; }
        public int? ExtraServiceId { get; init; }
    }
}
