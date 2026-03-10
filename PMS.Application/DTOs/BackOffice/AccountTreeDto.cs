using PMS.Domain.Enums.BackOffice;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public class AccountTreeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public int Level { get; set; }
        public bool IsGroup { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; }
        public int? ParentAccountId { get; set; }

        public List<AccountTreeDto> Children { get; set; } = new();
    }
}
