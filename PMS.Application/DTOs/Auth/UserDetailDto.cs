using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Auth
{
    public class UserDetailDto : UserResponseDto
    {
        public string NationalId { get; set; }
        public string WorkNumber { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ProfileImagePath { get; set; }

        // قائمة بمسارات المستندات اللي رفعها
        public List<string> DocumentPaths { get; set; }
    }
}
