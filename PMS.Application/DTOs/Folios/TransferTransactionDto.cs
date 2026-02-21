using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Folios
{
    public class TransferTransactionDto
    {
        [Required]
        public int TargetReservationId { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}
