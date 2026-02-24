using System;
using System.Collections.Generic;
using PMS.Application.DTOs.Common;

namespace PMS.Application.DTOs.Reservations
{
    public record ReservationDto : BaseAuditableDto
    {
        public int Id { get; init; }
        public string ReservationNumber { get; init; } = string.Empty;

        // Guest
        public int GuestId { get; init; }
        public string GuestName { get; init; } = string.Empty;
        public string GuestPhone { get; init; } = string.Empty;
        public string? GuestEmail { get; init; }
        public string? GuestNationalId { get; init; }

        // Room
        public int RoomTypeId { get; init; }
        public string RoomTypeName { get; init; } = string.Empty;
        public int? RoomId { get; init; }
        public string? RoomNumber { get; init; }

        public int? CompanyId { get; init; }
        public string? CompanyName { get; init; }

        public int? RatePlanId { get; init; }
        public string? RatePlanName { get; init; }

        // Dates
        public DateTimeOffset CheckInDate { get; init; }
        public DateTimeOffset CheckOutDate { get; init; }
        public int Nights { get; init; }

        // Business
        public string RateCode { get; set; } = string.Empty;  // mutable: overwritten to CONFIDENTIAL by AfterMap
        public string MealPlan { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public int BookingSourceId { get; init; }
        public int MarketSegmentId { get; init; }
        public int MealPlanId { get; init; }

        // Financials — mutable so AfterMap can zero them for confidential rates
        public decimal NightlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ServicesAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public bool IsNoExtend { get; init; }
        public bool IsConfidentialRate { get; init; }

        public string Status { get; init; } = string.Empty;
        public string? Notes { get; init; }

        public string? ExternalReference { get; init; }
        public string? CarPlate { get; init; }
        public string? PurposeOfVisit { get; init; }
        public string? MarketSegment { get; init; }

        // Services
        public List<ReservationServiceDto> Services { get; init; } = new();
    }
}
