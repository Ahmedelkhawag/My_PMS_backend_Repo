using System.Collections.Generic;

namespace PMS.Application.DTOs.Configuration
{
    public class EnumLookupDto
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty; // Hex format
    }

    public class StatusConfigurationDto
    {
        public List<EnumLookupDto> HkStatuses { get; set; } = new List<EnumLookupDto>();
        public List<EnumLookupDto> FoStatuses { get; set; } = new List<EnumLookupDto>();
        public List<EnumLookupDto> BedTypes { get; set; } = new List<EnumLookupDto>();
    }
}
