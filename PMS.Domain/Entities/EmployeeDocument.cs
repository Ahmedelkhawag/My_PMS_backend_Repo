using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities
{
    public class EmployeeDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? FileType { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
