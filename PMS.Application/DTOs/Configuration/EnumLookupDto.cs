using System.Collections.Generic;

namespace PMS.Application.DTOs.Configuration
{
    public record EnumLookupDto
    {
        public int Value { get; init; }
        public string Name { get; init; } = string.Empty;
        public string ColorCode { get; init; } = string.Empty;
    }

    public record StatusConfigurationDto
    {
        public List<EnumLookupDto> HkStatuses { get; init; } = new();
        public List<EnumLookupDto> FoStatuses { get; init; } = new();
        public List<EnumLookupDto> BedTypes { get; init; } = new();
    }
}
