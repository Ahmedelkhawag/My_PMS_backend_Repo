using PMS.Domain.Enums.BackOffice;

namespace PMS.Application.DTOs.BackOffice.AR
{
    public record ReceiveARPaymentDto(
        int CompanyId,
        decimal Amount,
        ARPaymentMethod Method,
        string ReferenceNo,
        string? Remarks
    );
}
