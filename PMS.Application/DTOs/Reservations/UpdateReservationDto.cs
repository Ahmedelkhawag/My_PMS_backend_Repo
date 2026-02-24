using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.Reservations
{
    public record UpdateReservationDto
    {
        public int? GuestId { get; init; }
        public int? RoomTypeId { get; init; }
        public int? RoomId { get; init; }
        public int? CompanyId { get; init; }
        public int? RatePlanId { get; init; }
        public DateTimeOffset? CheckInDate { get; init; }
        public DateTimeOffset? CheckOutDate { get; init; }
        public decimal? NightlyRate { get; init; }
        public decimal? DiscountAmount { get; init; }
        public string? RateCode { get; init; }
        public int? MealPlanId { get; init; }
        public int? BookingSourceId { get; init; }
        public int? MarketSegmentId { get; init; }
        public string? PurposeOfVisit { get; init; }
        public string? Notes { get; init; }
        public string? ExternalReference { get; init; }
        public string? CarPlate { get; init; }
        public int? Adults { get; init; }
        public int? Children { get; init; }
        public bool? IsPostMaster { get; init; }
        public bool? IsGuestPay { get; init; }
        public bool? IsNoExtend { get; init; }
        public bool? IsConfidentialRate { get; init; }
        public bool? IsRateOverridden { get; init; }
        public List<CreateReservationServiceDto>? Services { get; init; }
    }
}
