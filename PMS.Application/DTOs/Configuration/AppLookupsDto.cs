using System.Collections.Generic;

namespace PMS.Application.DTOs.Configuration
{
    /// <summary>
    /// Aggregated application lookups used by the frontend to bootstrap dropdowns.
    /// </summary>
    public record AppLookupsDto
    {
        public List<LookupDto> RoomTypes { get; init; } = new();
        public List<BookingSourceLookupDto> BookingSources { get; init; } = new();
        public List<LookupDto> MarketSegments { get; init; } = new();
        public List<MealPlanLookupDto> MealPlans { get; init; } = new();
        public List<ExtraServiceLookupDto> ExtraServices { get; init; } = new();
        public List<EnumLookupDto> HkStatuses { get; init; } = new();
        public List<EnumLookupDto> FoStatuses { get; init; } = new();
        public List<EnumLookupDto> BedTypes { get; init; } = new();
        public List<EnumLookupDto> TransactionTypes { get; init; } = new();
        public List<LookupDto> ReservationStatuses { get; init; } = new();
        public List<RatePlanLookupDto> RatePlans { get; init; } = new();
    }
}
