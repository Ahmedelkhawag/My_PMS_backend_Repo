using PMS.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities.BackOffice
{
    public class CostCenter : BaseAuditableEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? ParentCostCenterId { get; set; }

        public virtual CostCenter? ParentCostCenter { get; set; }

        public bool IsGroup { get; set; }

        public virtual ICollection<CostCenter> ChildCostCenters { get; set; } = new List<CostCenter>();
        
        public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
    }
}
