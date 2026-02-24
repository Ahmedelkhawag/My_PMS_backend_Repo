using System.Text.Json.Serialization;

namespace PMS.Application.DTOs.Rooms
{
    /// <summary>
    /// Room DTO for dashboard grid: FO/HK status, room type, and optional current reservation.
    /// </summary>
    public record RoomDto
    {
        public int Id { get; init; }
        public string RoomNumber { get; init; } = string.Empty;
        public int FloorNumber { get; init; }
        public string RoomTypeName { get; init; } = string.Empty;
        public string RoomTypeCode { get; init; } = string.Empty;
        /// <summary>Front Office status: OCCUPIED or VACANT (string enum).</summary>
        public string FoStatus { get; set; } = "VACANT";
        /// <summary>Housekeeping status: CLEAN, DIRTY, etc. (string enum).</summary>
        public string HkStatus { get; init; } = "DIRTY";
        /// <summary>Bed type: SINGLE, TWIN, QUEEN, KING (string enum).</summary>
        public string BedType { get; init; } = "SINGLE";
        public int MaxAdults { get; init; }
        public decimal BasePrice { get; init; }
        public string? Notes { get; init; }
        /// <summary>Active reservation id when the room is occupied; null when vacant.</summary>
        public int? CurrentReservationId { get; set; }
        /// <summary>Guest full name for the active in-house reservation; null when vacant.</summary>
        public string? GuestName { get; set; }
        /// <summary>Set only when FoStatus is OCCUPIED.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CurrentReservationDto? CurrentReservation { get; set; }
    }
}
