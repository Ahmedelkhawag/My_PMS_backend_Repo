namespace PMS.Application.Interfaces.Services;

/// <summary>
/// Generates the bilingual (Arabic/English) Guest Registration Card PDF for a reservation.
/// </summary>
public interface IRegistrationCardPdfService
{
    /// <summary>
    /// Generates the registration card PDF for the given reservation.
    /// </summary>
    /// <param name="reservationId">The reservation ID.</param>
    /// <param name="receptionistName">Display name of the current user (receptionist).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PDF content and suggested file name, or null if reservation not found.</returns>
    Task<(byte[] Content, string FileName)?> GenerateRegistrationCardAsync(
        int reservationId,
        string? receptionistName,
        CancellationToken cancellationToken = default);
}
