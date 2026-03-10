using PMS.Domain.Enums.BackOffice;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.BackOffice
{
    public class CreateAccountDto
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public int? ParentAccountId { get; set; }

        public bool IsGroup { get; set; }
    }
}
