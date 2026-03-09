using PMS.Domain.Enums.BackOffice.AP;

namespace PMS.Application.DTOs.BackOffice.AP
{
    public record VendorDto(
        int Id,
        string Name,
        string TaxId,
        string? ContactPerson,
        string? Email,
        string? Phone,
        VendorCreditTerm CreditTerms,
        int APAccountId,
        int? DefaultExpenseAccountId,
        bool IsActive
    );

    public record CreateVendorDto(
        string Name,
        string TaxId,
        string? ContactPerson,
        string? Email,
        string? Phone,
        VendorCreditTerm CreditTerms,
        int APAccountId,
        int? DefaultExpenseAccountId
    );

    public record UpdateVendorDto(
        string? Name,
        string? TaxId,
        string? ContactPerson,
        string? Email,
        string? Phone,
        VendorCreditTerm? CreditTerms,
        int? APAccountId,
        int? DefaultExpenseAccountId,
        bool? IsActive
    );
}
