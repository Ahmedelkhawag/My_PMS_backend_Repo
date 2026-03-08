namespace PMS.Application.DTOs.BackOffice.AR
{
    public record ARAgingReportDto(
        DateTime GeneratedAt,
        List<ARAgingBucketDto> Buckets,
        decimal GrandTotal
    );
}
