namespace PMS.Application.DTOs.Configuration
{
    public record RoomStatusLookupDto : LookupDto
    {
        public string Color { get; init; } = string.Empty;
    }
}
