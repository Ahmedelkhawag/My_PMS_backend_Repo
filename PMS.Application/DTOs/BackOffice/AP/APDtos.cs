using PMS.Domain.Enums.BackOffice;
using PMS.Domain.Enums.BackOffice.AP;
using System;
using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice.AP
{
    public record APPaymentAllocationInputDto
    {
        public int InvoiceId { get; init; }
        public decimal AllocatedAmount { get; init; }
    }

    public record CreateAPPaymentDto
    {
        public int VendorId { get; init; }
        public decimal Amount { get; init; }
        public ARPaymentMethod Method { get; init; }
        public string? ReferenceNo { get; init; }
        public int CreditAccountId { get; init; }
        public List<APPaymentAllocationInputDto> Allocations { get; init; } = new();
    }

    public record APInvoiceLineDto
    {
        public int Id { get; init; }
        public int APInvoiceId { get; init; }
        public string Description { get; init; }
        public decimal Amount { get; init; }
        public int ExpenseAccountId { get; init; }
    }

    public record APInvoiceDto
    {
        public int Id { get; init; }
        public int VendorId { get; init; }
        public string VendorName { get; init; }
        public string VendorInvoiceNo { get; init; }
        public DateTime InvoiceDate { get; init; }
        public DateTime DueDate { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal AmountPaid { get; init; }
        public decimal Balance { get; init; }
        public APInvoiceStatus Status { get; init; }
        public int? JournalEntryId { get; init; }
        public IEnumerable<APInvoiceLineDto> Lines { get; init; }
    }

    public record APPaymentAllocationDto
    {
        public int Id { get; init; }
        public int APPaymentId { get; init; }
        public int APInvoiceId { get; init; }
        public decimal AllocatedAmount { get; init; }
    }

    public record APPaymentDto
    {
        public int Id { get; init; }
        public int VendorId { get; init; }
        public string VendorName { get; init; }
        public decimal Amount { get; init; }
        public DateTime PaymentDate { get; init; }
        public string Method { get; init; }
        public string? ReferenceNo { get; init; }
        public int? JournalEntryId { get; init; }
        public IEnumerable<APPaymentAllocationDto> Allocations { get; init; }
    }
}
