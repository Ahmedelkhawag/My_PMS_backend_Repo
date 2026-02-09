using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Rooms
{
    public class UpdateRoomDto : CreateRoomDto
    {
        public int Id { get; set; }

        public string Status { get; set; }
    }
}
