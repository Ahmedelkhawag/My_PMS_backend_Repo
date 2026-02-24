using System;

namespace PMS.Application.DTOs.Common
{
    public record StatusDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
