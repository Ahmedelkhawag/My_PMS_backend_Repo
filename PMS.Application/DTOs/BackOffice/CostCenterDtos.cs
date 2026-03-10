using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public class CostCenterDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? ParentCostCenterId { get; set; }
        public bool IsGroup { get; set; }
        public List<CostCenterDto> Children { get; set; } = new List<CostCenterDto>();
    }

    public record CreateCostCenterDto(
        string Code,
        string Name,
        int? ParentCostCenterId,
        bool IsGroup
    );
}
