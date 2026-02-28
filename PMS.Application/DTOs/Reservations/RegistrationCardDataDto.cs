namespace PMS.Application.DTOs.Reservations;

/// <summary>
/// Data required to render the bilingual Guest Registration Card PDF.
/// </summary>
public record RegistrationCardDataDto
{
    public string ReservationNumber { get; init; } = string.Empty;
    public string GuestName { get; init; } = string.Empty;
    public string PassportOrIdNumber { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public string RoomTypeName { get; init; } = string.Empty;
    public string CheckInDateFormatted { get; init; } = string.Empty;
    public string CheckOutDateFormatted { get; init; } = string.Empty;
    public int Adults { get; init; }
    public int Children { get; init; }
    public decimal NightlyRate { get; init; }
    public string Currency { get; init; } = "EGP";
    public string? ReceptionistName { get; init; }
    public string TodayFormatted { get; init; } = string.Empty;
    public string? HotelName { get; init; }
}
