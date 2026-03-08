namespace PMS.Application.DTOs.BackOffice.AR
{
    public record CompanyStatementReportDto(
        string CompanyName,
        DateTime StartDate,
        DateTime EndDate,
        decimal OpeningBalance,
        decimal ClosingBalance,
        List<CompanyStatementLineDto> Lines
    );
}
