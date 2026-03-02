using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.Domain.Entities
{
    public class RoomType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; 

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; } 

        public string? Description { get; set; } 

        public int MaxAdults { get; set; } 
        public int MaxChildren { get; set; } 

        
        
        public ICollection<Room> Rooms { get; set; }
    }
}
