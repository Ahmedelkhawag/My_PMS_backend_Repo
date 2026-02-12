namespace PMS.Application.DTOs.Rooms
{
    /// <summary>
    /// Partial update: send only the fields you want to change.
    /// Omitted fields will be left unchanged.
    /// </summary>
    public class UpdateRoomDto
    {
        public string? RoomNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, 100, ErrorMessage = "رقم الطابق غير صحيح")]
        public int? FloorNumber { get; set; }

        public int? RoomTypeId { get; set; }

        public string? Notes { get; set; }

        public string? Status { get; set; }
    }
}
