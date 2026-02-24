namespace PMS.Application.DTOs.Configuration
{
    public record LookupDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
