using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PMS.Application.DTOs.Rooms
{
    public class CreateRoomDto
    {
        [Required(ErrorMessage = "رقم الغرفة مطلوب")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الطابق مطلوب")]
        [Range(1, 100, ErrorMessage = "رقم الطابق غير صحيح")]
        public int FloorNumber { get; set; }

        [Required(ErrorMessage = "نوع الغرفة مطلوب")]
        public int RoomTypeId { get; set; }

        public string? Notes { get; set; }

    }
}
