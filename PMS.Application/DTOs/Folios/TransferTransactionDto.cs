using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Folios
{
    public record TransferTransactionDto
    {
        [Required]
        public int TargetReservationId { get; init; }

        [Required]
        public string Reason { get; init; } = string.Empty;
    }
}
