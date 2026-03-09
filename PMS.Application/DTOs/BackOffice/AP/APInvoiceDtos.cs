using PMS.Domain.Enums.BackOffice.AP;
using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice.AP
{
    // ─── Create ─────────────────────────────────────────────────────────────────
    public record CreateAPInvoiceLineDto(
        string Description,
        decimal Amount,
        int ExpenseAccountId
    );

    public record CreateAPInvoiceDto(
        int VendorId,
        string VendorInvoiceNo,
        DateTime InvoiceDate,
        IEnumerable<CreateAPInvoiceLineDto> Lines
    );
}
