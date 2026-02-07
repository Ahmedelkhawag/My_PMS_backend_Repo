using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Rooms
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }


        public string Status { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty; // اسم النوع (Single)
        public decimal Price { get; set; } // السعر
        public int MaxAdults { get; set; }
    }
}
