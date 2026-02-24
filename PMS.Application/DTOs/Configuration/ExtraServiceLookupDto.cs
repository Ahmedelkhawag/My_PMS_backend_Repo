namespace PMS.Application.DTOs.Configuration
{
    public record ExtraServiceLookupDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public bool IsPerDay { get; init; }
    }
}
