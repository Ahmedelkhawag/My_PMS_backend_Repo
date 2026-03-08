using PMS.Domain.Enums.BackOffice;

namespace PMS.Application.DTOs.BackOffice.AR
{
    public record CreateARAdjustmentDto(
        int InvoiceId,
        decimal Amount,
        ARAdjustmentType Type,
        string Reason
    );
}
