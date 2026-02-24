using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs.Reservations
{
    public record CreateReservationDto
    {
        [Required(ErrorMessage = "النزيل مطلوب")]
        public int GuestId { get; init; }

        [Required(ErrorMessage = "نوع الغرفة مطلوب")]
        public int RoomTypeId { get; init; }

        public int? RoomId { get; init; }
        public int? CompanyId { get; init; }
        public int? RatePlanId { get; init; }

        [Required]
        public DateTimeOffset CheckInDate { get; init; }

        [Required]
        public DateTimeOffset CheckOutDate { get; init; }

        [Required(ErrorMessage = "سعر الليلة مطلوب")]
        public decimal NightlyRate { get; init; }

        public string RateCode { get; init; } = "Standard";

        /// <summary>Allows overriding the calculated rate from the selected Rate Plan.</summary>
        public bool IsRateOverridden { get; init; }

        [Required(ErrorMessage = "خطة الوجبات مطلوبة")]
        public int MealPlanId { get; set; }

        public bool IsPostMaster { get; init; }
        public bool IsGuestPay { get; init; }
        public bool IsNoExtend { get; init; }
        public bool IsConfidentialRate { get; init; }
        public bool IsWalkIn { get; init; }

        public List<CreateReservationServiceDto>? Services { get; init; }

        public int Adults { get; init; } = 1;
        public int Children { get; init; }
        public string? Notes { get; init; }

        public decimal DiscountAmount { get; init; }
        public string? PurposeOfVisit { get; init; }
        public int BookingSourceId { get; init; }
        public int MarketSegmentId { get; set; }
        public string? ExternalReference { get; init; }
        public string? CarPlate { get; init; }
    }
}
