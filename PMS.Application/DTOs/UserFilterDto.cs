namespace PMS.Application.DTOs
{
    public record UserFilterDto
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Search { get; init; }

        public string? Role { get; init; }
        public bool? IsActive { get; init; }

        /// <summary>
        /// When true, returns only closed items that have a non-zero discrepancy (Difference).
        /// </summary>
        public bool? ShowOnlyDiscrepancies { get; init; }
    }
}
