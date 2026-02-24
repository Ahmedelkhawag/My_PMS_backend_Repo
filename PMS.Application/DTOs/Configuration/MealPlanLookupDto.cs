namespace PMS.Application.DTOs.Configuration
{
    public record MealPlanLookupDto : LookupDto
    {
        public decimal Price { get; init; }
    }
}
