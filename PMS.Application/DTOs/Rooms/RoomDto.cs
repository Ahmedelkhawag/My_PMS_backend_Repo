using System.Text.Json.Serialization;

namespace PMS.Application.DTOs.Rooms
{
    /// <summary>
    /// Room DTO for dashboard grid: FO/HK status, room type, and optional current reservation.
    /// </summary>
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomTypeCode { get; set; } = string.Empty;
        /// <summary>Front Office status: OCCUPIED or VACANT (string enum).</summary>
        public string FoStatus { get; set; } = "VACANT";
        /// <summary>Housekeeping status: CLEAN, DIRTY, etc. (string enum).</summary>
        public string HkStatus { get; set; } = "DIRTY";
        /// <summary>Bed type: SINGLE, TWIN, QUEEN, KING (string enum).</summary>
        public string BedType { get; set; } = "SINGLE";
        public int MaxAdults { get; set; }
        public decimal BasePrice { get; set; }
		public string? Notes { get; set; }
        /// <summary>Set only when FoStatus is OCCUPIED.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CurrentReservationDto? CurrentReservation { get; set; }
    }
}
