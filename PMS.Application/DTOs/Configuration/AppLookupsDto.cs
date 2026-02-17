using System.Collections.Generic;

namespace PMS.Application.DTOs.Configuration
{
    /// <summary>
    /// Aggregated application lookups used by the frontend to bootstrap dropdowns.
    /// </summary>
    public class AppLookupsDto
    {
        public List<LookupDto> RoomTypes { get; set; } = new List<LookupDto>();
        public List<BookingSourceLookupDto> BookingSources { get; set; } = new List<BookingSourceLookupDto>();
        public List<LookupDto> MarketSegments { get; set; } = new List<LookupDto>();
        public List<MealPlanLookupDto> MealPlans { get; set; } = new List<MealPlanLookupDto>();
        public List<ExtraServiceLookupDto> ExtraServices { get; set; } = new List<ExtraServiceLookupDto>();
        public List<EnumLookupDto> HkStatuses { get; set; } = new List<EnumLookupDto>();
        public List<EnumLookupDto> FoStatuses { get; set; } = new List<EnumLookupDto>();
        public List<EnumLookupDto> BedTypes { get; set; } = new List<EnumLookupDto>();
        public List<EnumLookupDto> TransactionTypes { get; set; } = new List<EnumLookupDto>();
        public List<LookupDto> ReservationStatuses { get; set; } = new List<LookupDto>();
    }
}

