using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities.Configuration
{
    public class Currency : BaseAuditableEntity
    {
        [Required]
        [MaxLength(3)]
        public string Code { get; set; } = string.Empty; // e.g., USD, EGP

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal CurrentExchangeRate { get; set; } = 1m;

        public bool IsActive { get; set; } = true;
    }
}
