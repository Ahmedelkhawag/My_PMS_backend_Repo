using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Enums
{
    /// <summary>
    /// Types of folio transactions.
    /// 
    /// Debit transactions MUST be in the range 10–19.
    /// Credit transactions MUST be in the range 20–29.
    /// </summary>
    public enum TransactionType
    {
        // 10–19 : Debits (increase charges)
        RoomCharge = 10,
        ServiceCharge = 11,
        TaxCharge = 12,
        TransferDebit = 13,
        CancellationFee = 14,
        EarlyDepartureFee = 15,

        // 20–29 : Credits (increase payments)
        CashPayment = 20,
        CardPayment = 21,
        BankTransferPayment = 22,
        AdjustmentCredit = 23,
        Discount = 24,
        Refund = 25,
        TransferCredit = 26,
        CityLedgerPayment = 27
    }
}

