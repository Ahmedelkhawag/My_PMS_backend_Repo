namespace PMS.Application.DTOs.BackOffice.AR
{
    public record ARAgingBucketDto(
        int CompanyId,
        string CompanyName,
        decimal Current0to30,
        decimal Over30,
        decimal Over60,
        decimal Over90,
        decimal TotalOutstanding
    );
}
