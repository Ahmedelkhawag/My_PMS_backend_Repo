namespace PMS.Application.DTOs.Configuration
{
    public record BookingSourceLookupDto : LookupDto
    {
        public bool RequiresExternalReference { get; init; }
    }
}
