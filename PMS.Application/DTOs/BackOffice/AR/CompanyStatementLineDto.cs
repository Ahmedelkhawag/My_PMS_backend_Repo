namespace PMS.Application.DTOs.BackOffice.AR
{
    public record CompanyStatementLineDto(
        DateTime Date,
        string ReferenceNo,
        string Description,
        decimal Debit,
        decimal Credit,
        decimal Balance
    );
}
