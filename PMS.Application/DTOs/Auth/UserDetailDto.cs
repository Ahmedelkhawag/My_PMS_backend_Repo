using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.Auth
{
    public record UserDetailDto : UserResponseDto
    {
        public string? NationalId { get; init; }
        public string? WorkNumber { get; init; }
        public string? Nationality { get; init; }
        public string? Gender { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? ProfileImagePath { get; init; }
        public List<string> DocumentPaths { get; set; } = new();
    }
}
