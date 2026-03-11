using PMS.Domain.Enums.BackOffice;

namespace PMS.Application.DTOs.BackOffice.AR
{
    public record ProcessPaymentDto(
        int CompanyId,
        decimal Amount,
        ARPaymentMethod Method,
        string ReferenceNo,
        string? Remarks,
        int? InvoiceId
    );

    public record AllocationRequest(
        int InvoiceId,
        decimal Amount
    );
}
