using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.BackOffice;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Entities.BackOffice.AR;
using PMS.Domain.Entities.BackOffice.AP;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMS.Application.Exceptions;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<bool>> PostTransactionToGLAsync(int folioTransactionId)
        {
            var transaction = await _unitOfWork.FolioTransactions
                .GetQueryable()
                .FirstOrDefaultAsync(t => t.Id == folioTransactionId);

            if (transaction == null)
            {
                return new ApiResponse<bool>("Folio transaction not found.");
            }

            if (transaction.IsVoided)
            {
                return new ApiResponse<bool>("Cannot post a voided transaction.");
            }

            var mappings = await _unitOfWork.JournalEntryMappings
                .GetQueryable()
                .Where(m => m.TransactionType == transaction.Type && m.IsActive)
                .ToListAsync();

            if (!mappings.Any())
            {
                return new ApiResponse<bool>($"No journal entry mapping configured for transaction type '{transaction.Type}'.");
            }

            var totalPercentage = mappings.Sum(m => m.Percentage);
            if (totalPercentage != 100m)
            {
                return new ApiResponse<bool>($"Total mapping percentage for {transaction.Type} must equal 100%. Current: {totalPercentage}%.");
            }

            var absAmount = Math.Abs(transaction.Amount);
            if (absAmount == 0)
            {
                return new ApiResponse<bool>("Transaction amount is zero; nothing to post.");
            }

            var businessDate = transaction.BusinessDate;

            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<bool>("No BusinessDay found for the transaction business date.");
            }

            var journalEntry = new JournalEntry
            {
                EntryNumber = GenerateEntryNumber(businessDate),
                Date = businessDate,
                Description = transaction.Description,
                ReferenceNo = transaction.ReferenceNo,
                BusinessDayId = businessDay.Id,
                Status = JournalEntryStatus.PendingApproval
            };

            foreach (var mapping in mappings)
            {
                var portionAmount = Math.Round(absAmount * (mapping.Percentage / 100m), 2);

                journalEntry.Lines.Add(new JournalEntryLine
                {
                    AccountId = mapping.DebitAccountId,
                    Debit = portionAmount,
                    Credit = 0m,
                    Memo = transaction.Description,
                    CostCenterId = mapping.CostCenterId
                });

                journalEntry.Lines.Add(new JournalEntryLine
                {
                    AccountId = mapping.CreditAccountId,
                    Debit = 0m,
                    Credit = portionAmount,
                    Memo = transaction.Description,
                    CostCenterId = mapping.CostCenterId
                });
            }

            // Group identically mapped accounts
            var groupedLines = journalEntry.Lines
                .GroupBy(l => new { l.AccountId, l.CostCenterId })
                .Select(g => new JournalEntryLine
                {
                    AccountId = g.Key.AccountId,
                    CostCenterId = g.Key.CostCenterId,
                    Debit = g.Sum(l => l.Debit),
                    Credit = g.Sum(l => l.Credit),
                    Memo = g.First().Memo
                }).ToList();

            journalEntry.Lines = groupedLines;

            await ValidateJournalEntryAsync(journalEntry);

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                await _unitOfWork.CompleteAsync(); // Generate journalEntry.Id

                transaction.IsPostedToGL = true;
                transaction.JournalEntryId = journalEntry.Id;
                _unitOfWork.FolioTransactions.Update(transaction);

                // Phase 2: Balances are explicitly NOT updated here; they await Approval.
                await _unitOfWork.CompleteAsync();
                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "Transaction generated Journal Entry successfully and is Pending Approval.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<bool>> ReverseTransactionInGLAsync(int folioTransactionId)
        {
            var transaction = await _unitOfWork.FolioTransactions
                .GetQueryable()
                .Include(t => t.Folio)
                .FirstOrDefaultAsync(t => t.Id == folioTransactionId);

            if (transaction == null)
            {
                return new ApiResponse<bool>("Folio transaction not found.");
            }

            if (!transaction.IsPostedToGL || transaction.JournalEntryId == null)
            {
                // Not posted, so nothing to reverse in GL
                return new ApiResponse<bool>(true, "Transaction was not posted to GL; nothing to reverse.");
            }

            var originalJournalEntry = await _unitOfWork.JournalEntries
                .GetQueryable()
                .Include(je => je.Lines)
                .FirstOrDefaultAsync(je => je.Id == transaction.JournalEntryId);

            if (originalJournalEntry == null)
            {
                return new ApiResponse<bool>("Original Journal Entry not found.");
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<bool>("No open BusinessDay found to attach the reversal journal entry.");
            }

            var reversalJournalEntry = new JournalEntry
            {
                EntryNumber = GenerateEntryNumber(businessDate),
                Date = DateTime.UtcNow,
                ReferenceNo = $"REV-{transaction.Id}",
                Description = $"Reversal of Transaction #{transaction.Id} - Original Ref: {transaction.ReferenceNo ?? "N/A"}",
                BusinessDayId = businessDay.Id
            };

            foreach (var line in originalJournalEntry.Lines)
            {
                // Swap Debit and Credit
                var reversedLine = new JournalEntryLine
                {
                    JournalEntry = reversalJournalEntry,
                    AccountId = line.AccountId,
                    Debit = line.Credit,
                    Credit = line.Debit,
                    Memo = $"Reversal: {line.Memo}"
                };

                reversalJournalEntry.Lines.Add(reversedLine);
            }

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(reversalJournalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(reversalJournalEntry.Lines);

                // Update balances applying the NEW reversal lines
                foreach (var line in reversalJournalEntry.Lines)
                {
                    var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
                    if (account != null)
                    {
                        if (line.Debit > 0)
                        {
                            ApplyBalanceEffect(account, line.Debit, isDebit: true);
                        }
                        if (line.Credit > 0)
                        {
                            ApplyBalanceEffect(account, line.Credit, isDebit: false);
                        }
                        _unitOfWork.Accounts.Update(account);
                    }
                }

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "Transaction reversed in GL successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // AP Invoice → GL
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<int>> PostAPInvoiceToGLAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.APInvoices
                .GetQueryable()
                .Include(i => i.Lines)
                .Include(i => i.Vendor)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                return new ApiResponse<int>("AP Invoice not found.");

            if (invoice.TotalAmount <= 0)
                return new ApiResponse<int>("Invoice total is zero; nothing to post.");

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
                return new ApiResponse<int>("No open BusinessDay found to attach the journal entry.");

            var description = $"AP Invoice #{invoice.VendorInvoiceNo} from {invoice.Vendor.Name}";

            var journalEntry = new JournalEntry
            {
                EntryNumber   = GenerateEntryNumber(businessDate),
                Date          = invoice.InvoiceDate,
                Description   = description,
                ReferenceNo   = invoice.VendorInvoiceNo,
                BusinessDayId = businessDay.Id
            };

            // Credit line → AP Control Account (Vendor.APAccountId)
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = invoice.Vendor.APAccountId,
                Debit     = 0m,
                Credit    = invoice.TotalAmount,
                Memo      = description
            });

            // Debit lines → one per invoice line (Expense Account)
            foreach (var line in invoice.Lines)
            {
                journalEntry.Lines.Add(new JournalEntryLine
                {
                    AccountId = line.ExpenseAccountId,
                    Debit     = line.Amount,
                    Credit    = 0m,
                    Memo      = line.Description
                });
            }

            // Gather all account IDs to load in one query
            var accountIds = journalEntry.Lines.Select(l => l.AccountId).Distinct().ToList();
            var accounts   = await _unitOfWork.Accounts
                .GetQueryable()
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();

            await ValidateJournalEntryAsync(journalEntry);

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);
                await _unitOfWork.CompleteAsync(); // generate journalEntry.Id

                foreach (var jeLine in journalEntry.Lines)
                {
                    var account = accounts.FirstOrDefault(a => a.Id == jeLine.AccountId);
                    if (account != null)
                    {
                        if (jeLine.Debit > 0)  ApplyBalanceEffect(account, jeLine.Debit,  isDebit: true);
                        if (jeLine.Credit > 0) ApplyBalanceEffect(account, jeLine.Credit, isDebit: false);
                        _unitOfWork.Accounts.Update(account);
                    }
                }

                // Link the journal entry back to the invoice
                invoice.JournalEntryId = journalEntry.Id;
                _unitOfWork.APInvoices.Update(invoice);

                await _unitOfWork.CompleteAsync();
                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<int>(journalEntry.Id, "AP Invoice posted to GL successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Generic Journal Entry Reversal (reusable by AR voiding & AP voiding)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> ReverseJournalEntryAsync(
            int journalEntryId, string referenceCode, string description)
        {
            var originalEntry = await _unitOfWork.JournalEntries
                .GetQueryable()
                .Include(je => je.Lines)
                .FirstOrDefaultAsync(je => je.Id == journalEntryId);

            if (originalEntry == null)
                return new ApiResponse<bool>("Journal Entry not found.");

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay  = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
                return new ApiResponse<bool>("No open BusinessDay found to attach the reversal entry.");

            var reversalEntry = new JournalEntry
            {
                EntryNumber   = GenerateEntryNumber(businessDate),
                Date          = DateTime.UtcNow,
                ReferenceNo   = referenceCode,
                Description   = description,
                BusinessDayId = businessDay.Id
            };

            foreach (var line in originalEntry.Lines)
            {
                reversalEntry.Lines.Add(new JournalEntryLine
                {
                    JournalEntry = reversalEntry,
                    AccountId    = line.AccountId,
                    Debit        = line.Credit,   // swap
                    Credit       = line.Debit,    // swap
                    Memo         = $"Reversal: {line.Memo}"
                });
            }

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(reversalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(reversalEntry.Lines);

                foreach (var line in reversalEntry.Lines)
                {
                    var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
                    if (account == null) continue;
                    if (line.Debit > 0) ApplyBalanceEffect(account, line.Debit, isDebit: true);
                    if (line.Credit > 0) ApplyBalanceEffect(account, line.Credit, isDebit: false);
                    _unitOfWork.Accounts.Update(account);
                }

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
                return new ApiResponse<bool>(true, "Journal entry reversed successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<bool>> PostARPaymentToGLAsync(int arPaymentId)
        {
            var payment = await _unitOfWork.ARPayments
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == arPaymentId);

            if (payment == null)
            {
                return new ApiResponse<bool>("AR payment not found.");
            }

            if (payment.Amount <= 0)
            {
                return new ApiResponse<bool>("AR payment amount is zero; nothing to post.");
            }

            var debitAccountId = GetDebitAccountIdForPaymentMethod(payment.PaymentMethod);
            if (debitAccountId == null)
            {
                return new ApiResponse<bool>($"Unknown payment method '{payment.PaymentMethod}'; cannot determine debit account.");
            }

            const int AdvanceDepositAccountId = 2131;
            var debitAccount = await _unitOfWork.Accounts.GetByIdAsync(debitAccountId.Value);
            var creditAccount = await _unitOfWork.Accounts.GetByIdAsync(AdvanceDepositAccountId);

            if (debitAccount == null || creditAccount == null)
            {
                return new ApiResponse<bool>("Debit or Advance Deposit credit account not found.");
            }

            if (debitAccount.IsGroup || creditAccount.IsGroup)
            {
                throw new InvalidOperationException("Cannot post journal entries to group accounts.");
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<bool>("No open BusinessDay found to attach the journal entry.");
            }

            var description = $"AR Payment - Ref: {payment.ReferenceNumber ?? "N/A"}";
            var journalEntry = new JournalEntry
            {
                EntryNumber = GenerateEntryNumber(businessDate),
                Date = businessDate,
                Description = description,
                ReferenceNo = payment.ReferenceNumber ?? string.Empty,
                BusinessDayId = businessDay.Id
            };

            var absAmount = Math.Abs(payment.Amount);
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = debitAccountId.Value,
                Debit = absAmount,
                Credit = 0m,
                Memo = description
            });
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = AdvanceDepositAccountId,
                Debit = 0m,
                Credit = absAmount,
                Memo = description
            });

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                ApplyBalanceEffect(debitAccount, absAmount, isDebit: true);
                ApplyBalanceEffect(creditAccount, absAmount, isDebit: false);

                _unitOfWork.Accounts.Update(debitAccount);
                _unitOfWork.Accounts.Update(creditAccount);

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "AR payment posted to GL successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<bool>> PostARAllocationToGLAsync(int arAllocationId)
        {
            var allocation = await _unitOfWork.ARAllocations
                .GetQueryable()
                .Include(a => a.Payment)
                .Include(a => a.Invoice)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == arAllocationId);

            if (allocation == null)
            {
                return new ApiResponse<bool>("AR allocation not found.");
            }

            if (allocation.Amount <= 0)
            {
                return new ApiResponse<bool>("AR allocation amount is zero; nothing to post.");
            }

            const int AdvanceDepositAccountId = 2131;
            const int CityLedgerAccountId = 1131;

            var debitAccount = await _unitOfWork.Accounts.GetByIdAsync(AdvanceDepositAccountId);
            var creditAccount = await _unitOfWork.Accounts.GetByIdAsync(CityLedgerAccountId);

            if (debitAccount == null || creditAccount == null)
            {
                return new ApiResponse<bool>("Advance deposits or city ledger account not found.");
            }

            if (debitAccount.IsGroup || creditAccount.IsGroup)
            {
                throw new InvalidOperationException("Cannot post journal entries to group accounts.");
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<bool>("No open BusinessDay found to attach the journal entry.");
            }

            var description = $"AR Allocation - Pmt Ref: {allocation.Payment?.ReferenceNumber ?? "N/A"} to Inv: {allocation.Invoice?.InvoiceNumber ?? "N/A"}";
            var journalEntry = new JournalEntry
            {
                EntryNumber = GenerateEntryNumber(businessDate),
                Date = allocation.AllocatedDate,
                Description = description,
                ReferenceNo = allocation.Payment?.ReferenceNumber ?? string.Empty,
                BusinessDayId = businessDay.Id
            };

            var absAmount = Math.Abs(allocation.Amount);
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = AdvanceDepositAccountId,
                Debit = absAmount,
                Credit = 0m,
                Memo = description
            });
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = CityLedgerAccountId,
                Debit = 0m,
                Credit = absAmount,
                Memo = description
            });

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                // Note: Balance effects might be omitted here if we wait for Journal Entry approval
                // per maker-checker workflow, but keeping consistent with original logic first:
                ApplyBalanceEffect(debitAccount, absAmount, isDebit: true);
                ApplyBalanceEffect(creditAccount, absAmount, isDebit: false);

                _unitOfWork.Accounts.Update(debitAccount);
                _unitOfWork.Accounts.Update(creditAccount);

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "AR allocation posted to GL successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<int>> PostAPPaymentToGLAsync(int paymentId, int creditAccountId)
        {
            var payment = await _unitOfWork.APPayments
                .GetQueryable()
                .Include(p => p.Vendor)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return new ApiResponse<int>("AP payment not found.");
            }

            if (payment.Amount <= 0)
            {
                return new ApiResponse<int>("AP payment amount is zero; nothing to post.");
            }

            var apControlAccount = await _unitOfWork.Accounts.GetByIdAsync(payment.Vendor.APAccountId);
            var creditAccount = await _unitOfWork.Accounts.GetByIdAsync(creditAccountId);

            if (apControlAccount == null || creditAccount == null)
            {
                return new ApiResponse<int>("AP control account or credit account not found.");
            }

            if (apControlAccount.IsGroup || creditAccount.IsGroup)
            {
                throw new InvalidOperationException("Cannot post journal entries to group accounts.");
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<int>("No open BusinessDay found to attach the journal entry.");
            }

            var description = $"Payment to Vendor {payment.Vendor.Name} - Ref: {payment.ReferenceNo ?? "N/A"}";

            var journalEntry = new JournalEntry
            {
                EntryNumber   = GenerateEntryNumber(businessDate),
                Date          = payment.PaymentDate,
                Description   = description,
                ReferenceNo   = payment.ReferenceNo ?? string.Empty,
                BusinessDayId = businessDay.Id
            };

            var absAmount = Math.Abs(payment.Amount);

            // Debit AP Control (reduce liability)
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = payment.Vendor.APAccountId,
                Debit     = absAmount,
                Credit    = 0m,
                Memo      = description
            });

            // Credit cash/bank account
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = creditAccountId,
                Debit     = 0m,
                Credit    = absAmount,
                Memo      = description
            });

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);
                await _unitOfWork.CompleteAsync(); // generate journalEntry.Id

                ApplyBalanceEffect(apControlAccount, absAmount, isDebit: true);
                ApplyBalanceEffect(creditAccount, absAmount, isDebit: false);

                _unitOfWork.Accounts.Update(apControlAccount);
                _unitOfWork.Accounts.Update(creditAccount);

                // Link back to APPayment
                var paymentToUpdate = await _unitOfWork.APPayments.GetByIdAsync(payment.Id);
                if (paymentToUpdate != null)
                {
                    paymentToUpdate.JournalEntryId = journalEntry.Id;
                    _unitOfWork.APPayments.Update(paymentToUpdate);
                }

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<int>(journalEntry.Id, "AP payment posted to GL successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        private static int? GetDebitAccountIdForPaymentMethod(string method)
        {
            return method?.ToUpperInvariant() switch
            {
                "CASH" => 1111,   // Front Office Cashier
                "CHECK" => 1121,  // Main Bank
                "BANKTRANSFER" => 1121,
                _ => null
            };
        }

        public async Task<ApiResponse<bool>> PostARAdjustmentToGLAsync(int arAdjustmentId)
        {
            var adjustment = await _unitOfWork.ARAdjustments
                .GetQueryable()
                .AsNoTracking()
                .Include(a => a.ARInvoice)
                .FirstOrDefaultAsync(a => a.Id == arAdjustmentId);

            if (adjustment == null)
            {
                return new ApiResponse<bool>("AR adjustment not found.");
            }

            if (adjustment.Amount <= 0)
            {
                return new ApiResponse<bool>("AR adjustment amount is zero; nothing to post.");
            }

            const int CityLedgerAccountId = 1131;
            const int RevenueAccountId = 411;
            const int DiscountContraRevenueAccountId = 413;

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<bool>("No open BusinessDay found to attach the journal entry.");
            }

            int debitAccountId;
            int creditAccountId;
            string description;

            if (adjustment.Type == Domain.Enums.BackOffice.ARAdjustmentType.CreditNote)
            {
                debitAccountId = DiscountContraRevenueAccountId;
                creditAccountId = CityLedgerAccountId;
                description = $"AR Credit Note - {adjustment.Reason}";
            }
            else
            {
                debitAccountId = CityLedgerAccountId;
                creditAccountId = RevenueAccountId;
                description = $"AR Debit Note - {adjustment.Reason}";
            }

            var debitAccount = await _unitOfWork.Accounts.GetByIdAsync(debitAccountId);
            var creditAccount = await _unitOfWork.Accounts.GetByIdAsync(creditAccountId);

            if (debitAccount == null || creditAccount == null)
            {
                return new ApiResponse<bool>("Debit or credit account not found.");
            }

            if (debitAccount.IsGroup || creditAccount.IsGroup)
            {
                throw new InvalidOperationException("Cannot post journal entries to group accounts.");
            }

            var journalEntry = new JournalEntry
            {
                EntryNumber = GenerateEntryNumber(businessDate),
                Date = businessDate,
                Description = description,
                ReferenceNo = adjustment.ReferenceNumber ?? string.Empty,
                BusinessDayId = businessDay.Id
            };

            var absAmount = Math.Abs(adjustment.Amount);
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = debitAccountId,
                Debit = absAmount,
                Credit = 0m,
                Memo = description
            });
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = creditAccountId,
                Debit = 0m,
                Credit = absAmount,
                Memo = description
            });

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                ApplyBalanceEffect(debitAccount, absAmount, isDebit: true);
                ApplyBalanceEffect(creditAccount, absAmount, isDebit: false);

                _unitOfWork.Accounts.Update(debitAccount);
                _unitOfWork.Accounts.Update(creditAccount);

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "AR adjustment posted to GL successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<bool>> PostTACommissionToGLAsync(int commissionRecordId)
        {
            var record = await _unitOfWork.TACommissionRecords
                .GetQueryable()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == commissionRecordId);

            if (record == null)
                return new ApiResponse<bool>("Commission record not found.");

            if (record.Status != CommissionStatus.Approved)
                return new ApiResponse<bool>("Commission record must be Approved before posting to GL.");

            if (record.JournalEntryId.HasValue)
                return new ApiResponse<bool>($"Commission already posted to GL (JE ID: {record.JournalEntryId}).");

            // Hardcoded IDs from ContextSeed
            const int commissionExpenseAccountId = 5151;
            const int commissionsPayableAccountId = 2141;

            var expenseAccount = await _unitOfWork.Accounts.GetByIdAsync(commissionExpenseAccountId);
            if (expenseAccount == null)
                return new ApiResponse<bool>($"GL Account {commissionExpenseAccountId} (TA Commission Expense) not found.");

            var payableAccount = await _unitOfWork.Accounts.GetByIdAsync(commissionsPayableAccountId);
            if (payableAccount == null)
                return new ApiResponse<bool>($"GL Account {commissionsPayableAccountId} (Commissions Payable) not found.");

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
                return new ApiResponse<bool>("No open BusinessDay found; cannot post GL entry.");

            var entryNumber = GenerateEntryNumber(businessDate);
            var description = $"TA Commission Approved – Company: {record.Company?.Name ?? record.CompanyId.ToString()}, Reservation: {record.ReservationId}";

            var journalEntry = new JournalEntry
            {
                EntryNumber = entryNumber,
                Date = businessDate,
                Description = description,
                Status = JournalEntryStatus.Posted,
                BusinessDayId = businessDay.Id
            };

            // Debit Commission Expense
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = commissionExpenseAccountId,
                Debit = record.CommissionAmount,
                Credit = 0m,
                Memo = description
            });

            // Credit Commissions Payable
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = commissionsPayableAccountId,
                Debit = 0m,
                Credit = record.CommissionAmount,
                Memo = description
            });

            // ── Architectural Fix: Validate against closed periods/integrity ──
            await ValidateJournalEntryAsync(journalEntry);

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }

            try
            {
                // Save Journal Entry
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                // Update account balances
                expenseAccount.CurrentBalance += record.CommissionAmount;
                payableAccount.CurrentBalance += record.CommissionAmount;
                _unitOfWork.Accounts.Update(expenseAccount);
                _unitOfWork.Accounts.Update(payableAccount);

                await _unitOfWork.CompleteAsync(); // Generate journalEntry.Id

                // Update Commission Record with the JE ID
                record.JournalEntryId = journalEntry.Id;
                _unitOfWork.TACommissionRecords.Update(record);
                
                await _unitOfWork.CompleteAsync();

                if (isLocalTransaction)
                    await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "Commission posted to GL successfully");
            }
            catch (Exception)
            {
                if (isLocalTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ApiResponse<bool>> CreateManualJournalEntryAsync(CreateJournalEntryDto dto)
        {
            if (dto == null || dto.Lines == null || dto.Lines.Count == 0)
            {
                return new ApiResponse<bool>("Journal entry must contain at least one line.");
            }

            var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();

            var businessDay = await _unitOfWork.BusinessDays
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Date == businessDate);

            if (businessDay == null)
            {
                return new ApiResponse<bool>("No open BusinessDay found to attach the journal entry.");
            }

            var journalEntry = _mapper.Map<JournalEntry>(dto);
            journalEntry.BusinessDayId = businessDay.Id;
            journalEntry.EntryNumber = GenerateEntryNumber(businessDate);
            journalEntry.Status = JournalEntryStatus.PendingApproval; // Phase 2: Set initial status

            foreach (var line in journalEntry.Lines)
            {
                if (line.CurrencyId.HasValue)
                {
                    line.ExchangeRate = line.ExchangeRate > 0 ? line.ExchangeRate : 1m;
                    line.Debit = line.DebitForeign * line.ExchangeRate;
                    line.Credit = line.CreditForeign * line.ExchangeRate;
                }
            }

            await ValidateJournalEntryAsync(journalEntry);

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                // CRITICAL CHANGE: ApplyBalanceEffect is NOT called during the initial creation.
                // The Account.CurrentBalance will be updated only when a separate approval action happens.

                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "Manual journal entry created and is pending approval.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<TrialBalanceReportDto>> GetTrialBalanceAsync()
        {
            var accounts = await _unitOfWork.Accounts
                .GetQueryable()
                .AsNoTracking()
                .Where(a => !a.IsGroup && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();

            var items = new List<TrialBalanceItemDto>();
            decimal totalDebit = 0m;
            decimal totalCredit = 0m;

            // Compute balances dynamically from JournalEntryLines where Status == Posted
            var postedLines = await _unitOfWork.JournalEntryLines
                .GetQueryable()
                .AsNoTracking()
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted)
                .GroupBy(l => l.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(l => l.Debit),
                    TotalCredit = g.Sum(l => l.Credit)
                })
                .ToDictionaryAsync(g => g.AccountId, g => g);

            foreach (var account in accounts)
            {
                var isDebitNormal = account.Type == AccountType.Asset || account.Type == AccountType.Expense;
                decimal accountDebit = 0m;
                decimal accountCredit = 0m;

                if (postedLines.TryGetValue(account.Id, out var lineTotals))
                {
                    var netBalance = isDebitNormal
                        ? lineTotals.TotalDebit - lineTotals.TotalCredit
                        : lineTotals.TotalCredit - lineTotals.TotalDebit;

                    if (isDebitNormal)
                    {
                        if (netBalance >= 0) accountDebit = netBalance;
                        else accountCredit = Math.Abs(netBalance);
                    }
                    else
                    {
                        if (netBalance >= 0) accountCredit = netBalance;
                        else accountDebit = Math.Abs(netBalance);
                    }
                }

                items.Add(new TrialBalanceItemDto(
                    account.Code,
                    account.NameEn,
                    accountDebit,
                    accountCredit
                ));
                totalDebit += accountDebit;
                totalCredit += accountCredit;
            }

            var isBalanced = totalDebit == totalCredit;
            var report = new TrialBalanceReportDto(items, totalDebit, totalCredit, isBalanced);

            return new ApiResponse<TrialBalanceReportDto>(report, "Trial balance retrieved successfully.");
        }

        public async Task<ApiResponse<int>> CreateAccountAsync(CreateAccountDto dto)
        {
            var exists = await _unitOfWork.Accounts.GetQueryable()
                .AnyAsync(a => a.Code == dto.Code);
            
            if (exists)
                return new ApiResponse<int>("Account Code already exists.");

            int level = 1;
            if (dto.ParentAccountId.HasValue)
            {
                var parent = await _unitOfWork.Accounts.GetByIdAsync(dto.ParentAccountId.Value);
                if (parent == null)
                    return new ApiResponse<int>("Parent account not found.");
                if (!parent.IsGroup)
                    return new ApiResponse<int>("Parent account must be a group account.");

                level = parent.Level + 1;
            }

            var account = _mapper.Map<Account>(dto);
            account.Level = level;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Accounts.AddAsync(account);
                
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<int>(account.Id, "Account created successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ApiResponse<List<AccountTreeDto>>> GetAccountsTreeAsync()
        {
            var accounts = await _unitOfWork.Accounts.GetQueryable()
                .AsNoTracking()
                .ToListAsync();

            var accountDict = accounts.Select(a => _mapper.Map<AccountTreeDto>(a))
                                      .ToDictionary(a => a.Id);

            var rootNodes = new List<AccountTreeDto>();

            foreach (var account in accountDict.Values)
            {
                if (account.ParentAccountId.HasValue && accountDict.TryGetValue(account.ParentAccountId.Value, out var parent))
                {
                    parent.Children.Add(account);
                }
                else
                {
                    if (account.ParentAccountId == null)
                    {
                        rootNodes.Add(account);
                    }
                }
            }

            return new ApiResponse<List<AccountTreeDto>>(rootNodes, "Tree retrieved successfully.");
        }

        public async Task<ApiResponse<int>> CreateCostCenterAsync(CreateCostCenterDto dto)
        {
            var exists = await _unitOfWork.CostCenters.GetQueryable()
                .AnyAsync(c => c.Code == dto.Code);
            
            if (exists)
                return new ApiResponse<int>("Cost Center Code already exists.");

            if (dto.ParentCostCenterId.HasValue)
            {
                var parent = await _unitOfWork.CostCenters.GetQueryable().FirstOrDefaultAsync(c => c.Id == dto.ParentCostCenterId.Value);
                if (parent == null)
                    return new ApiResponse<int>("Parent cost center not found.");
                if (!parent.IsGroup)
                    return new ApiResponse<int>("Parent cost center must be a group cost center.");
            }

            var costCenter = _mapper.Map<CostCenter>(dto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.CostCenters.AddAsync(costCenter);
                
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<int>(costCenter.Id, "Cost Center created successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ApiResponse<List<CostCenterDto>>> GetCostCentersTreeAsync()
        {
            var costCenters = await _unitOfWork.CostCenters.GetQueryable()
                .AsNoTracking()
                .ToListAsync();

            var costCenterDict = costCenters.Select(c => _mapper.Map<CostCenterDto>(c))
                                            .ToDictionary(c => c.Id);

            var rootNodes = new List<CostCenterDto>();

            foreach (var costCenter in costCenterDict.Values)
            {
                if (costCenter.ParentCostCenterId.HasValue && costCenterDict.TryGetValue(costCenter.ParentCostCenterId.Value, out var parent))
                {
                    parent.Children.Add(costCenter);
                }
                else
                {
                    if (costCenter.ParentCostCenterId == null)
                    {
                        rootNodes.Add(costCenter);
                    }
                }
            }

            return new ApiResponse<List<CostCenterDto>>(rootNodes, "Cost centers retrieved successfully.");
        }

        public async Task<ApiResponse<IEnumerable<int>>> GetUnpostedTransactionsAsync()
        {
            var unpostedIds = await _unitOfWork.FolioTransactions
                .GetQueryable()
                .AsNoTracking()
                .Where(t => t.Type == Domain.Enums.TransactionType.RoomCharge && !t.IsVoided && !t.IsPostedToGL)
                .Select(t => t.Id)
                .ToListAsync();

            return new ApiResponse<IEnumerable<int>>(unpostedIds, "Unposted transactions retrieved successfully.");
        }

        public async Task<ApiResponse<AccountStatementHeaderDto>> GetAccountStatementAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var account = await _unitOfWork.Accounts
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                return new ApiResponse<AccountStatementHeaderDto>("Account not found.");
            }

            if (account.IsGroup)
            {
                return new ApiResponse<AccountStatementHeaderDto>("Account statement is available only for leaf (non-group) accounts.");
            }

            var isDebitNormal = account.Type == AccountType.Asset || account.Type == AccountType.Expense;

            var allLinesQuery = _unitOfWork.JournalEntryLines
                .GetQueryable()
                .AsNoTracking()
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == accountId && l.JournalEntry.Status == JournalEntryStatus.Posted);

            var openingBalance = await allLinesQuery
                .Where(l => l.JournalEntry.Date < startDate)
                .SumAsync(l => isDebitNormal ? l.Debit - l.Credit : l.Credit - l.Debit);

            var statementLinesData = await allLinesQuery
                .Where(l => l.JournalEntry.Date >= startDate && l.JournalEntry.Date <= endDate)
                .OrderBy(l => l.JournalEntry.Date)
                .ThenBy(l => l.JournalEntryId)
                .ThenBy(l => l.Id)
                .Select(l => new
                {
                    l.JournalEntry.Date,
                    l.JournalEntry.EntryNumber,
                    Description = l.JournalEntry.Description ?? "",
                    l.Debit,
                    l.Credit
                })
                .ToListAsync();

            var lines = new List<AccountStatementLineDto>();
            var runningBalance = openingBalance;

            foreach (var row in statementLinesData)
            {
                var lineEffect = isDebitNormal ? row.Debit - row.Credit : row.Credit - row.Debit;
                runningBalance += lineEffect;

                lines.Add(new AccountStatementLineDto(
                    row.Date,
                    row.EntryNumber,
                    row.Description,
                    row.Debit,
                    row.Credit,
                    runningBalance
                ));
            }

            var closingBalance = runningBalance;
            var header = new AccountStatementHeaderDto(
                account.Code,
                account.NameEn,
                openingBalance,
                closingBalance,
                lines
            );

            return new ApiResponse<AccountStatementHeaderDto>(header, "Account statement retrieved successfully.");
        }

        private static (decimal Debit, decimal Credit) GetTrialBalanceDebitCredit(Account account)
        {
            var balance = account.CurrentBalance;
            var isDebitNormal = account.Type == AccountType.Asset || account.Type == AccountType.Expense;

            if (isDebitNormal)
            {
                if (balance >= 0)
                    return (balance, 0m);
                return (0m, Math.Abs(balance));
            }

            if (balance >= 0)
                return (0m, balance);
            return (Math.Abs(balance), 0m);
        }

        private static void ApplyBalanceEffect(Account account, decimal amount, bool isDebit)
        {
            var increaseOnDebit = account.Type == AccountType.Asset || account.Type == AccountType.Expense;

            if (isDebit)
            {
                account.CurrentBalance += increaseOnDebit ? amount : -amount;
            }
            else
            {
                account.CurrentBalance += increaseOnDebit ? -amount : amount;
            }
        }

        private static string GenerateEntryNumber(DateTime businessDate)
        {
            return $"JE-{businessDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
        }

        private async Task ValidateJournalEntryAsync(JournalEntry entry)
        {
            var period = await _unitOfWork.AccountingPeriods.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StartDate <= entry.Date.Date && p.EndDate >= entry.Date.Date);

            if (period == null)
                throw new BusinessException("Transaction Date does not belong to any defined accounting period.");

            if (period.Status == AccountingPeriodStatus.Closed || period.Status == AccountingPeriodStatus.Future)
                throw new BusinessException("Transaction Date belongs to a Closed or Future period.");

            decimal totalBaseDebit = 0m;
            decimal totalBaseCredit = 0m;

            var accountIds = entry.Lines.Select(l => l.AccountId).Distinct().ToList();
            var accounts = await _unitOfWork.Accounts.GetQueryable()
                .AsNoTracking()
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();

            if (accounts.Count != accountIds.Count)
                throw new BusinessException("One or more accounts do not exist.");

            foreach (var line in entry.Lines)
            {
                var account = accounts.FirstOrDefault(a => a.Id == line.AccountId);
                if (account != null)
                {
                    if (account.IsGroup)
                        throw new BusinessException("Journal entries can only be posted to non-group (leaf) accounts.");
                        
                    if ((account.Type == AccountType.Expense || account.Type == AccountType.Revenue) && !line.CostCenterId.HasValue)
                        throw new BusinessException("Cost Center is required for Expense/Revenue accounts.");
                }

                if (line.CurrencyId.HasValue && line.ExchangeRate <= 0)
                    throw new BusinessException("ExchangeRate must be greater than 0 for multi-currency lines.");

                totalBaseDebit += line.Debit;
                totalBaseCredit += line.Credit;
            }

            if (totalBaseDebit != totalBaseCredit)
                throw new BusinessException("Journal entry is not balanced. Total debit must equal total credit in base currency.");
        }

        public async Task<ApiResponse<bool>> ApproveJournalEntryAsync(int id, string userId)
        {
            var journalEntry = await _unitOfWork.JournalEntries
                .GetQueryable()
                .Include(je => je.Lines)
                .FirstOrDefaultAsync(je => je.Id == id);

            if (journalEntry == null)
            {
                return new ApiResponse<bool>("Journal entry not found.");
            }

            if (journalEntry.Status != JournalEntryStatus.PendingApproval)
            {
                return new ApiResponse<bool>("Journal entry is not in PendingApproval status.");
            }

            journalEntry.Status = JournalEntryStatus.Posted;
            journalEntry.ApprovedById = userId;
            journalEntry.ApprovedDate = DateTimeOffset.UtcNow;

            var accountIds = journalEntry.Lines.Select(l => l.AccountId).Distinct().ToList();
            var accounts = await _unitOfWork.Accounts.GetQueryable()
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();

            bool isLocalTransaction = false;
            if (!_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
                isLocalTransaction = true;
            }
            try
            {
                foreach (var line in journalEntry.Lines)
                {
                    var account = accounts.FirstOrDefault(a => a.Id == line.AccountId);
                    if (account != null)
                    {
                        var amount = line.Debit != 0 ? line.Debit : line.Credit;
                        var isDebit = line.Debit > 0;
                        ApplyBalanceEffect(account, amount, isDebit);
                        _unitOfWork.Accounts.Update(account);
                    }
                }

                _unitOfWork.JournalEntries.Update(journalEntry);
                await _unitOfWork.CompleteAsync();
                if (isLocalTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }

                return new ApiResponse<bool>(true, "Journal entry approved successfully.");
            }
            catch
            {
                if (isLocalTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<ApiResponse<bool>> RejectJournalEntryAsync(int id, string userId, string reason)
        {
            var journalEntry = await _unitOfWork.JournalEntries
                .GetQueryable()
                .FirstOrDefaultAsync(je => je.Id == id);

            if (journalEntry == null)
            {
                return new ApiResponse<bool>("Journal entry not found.");
            }

            if (journalEntry.Status != JournalEntryStatus.PendingApproval)
            {
                return new ApiResponse<bool>("Journal entry is not in PendingApproval status.");
            }

            journalEntry.Status = JournalEntryStatus.Rejected;
            journalEntry.RejectionReason = reason;

            // Save changes (do not affect account balances)
            _unitOfWork.JournalEntries.Update(journalEntry);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<bool>(true, "Journal entry rejected successfully.");
        }

        public async Task<ApiResponse<PnLReportDto>> GetPnLReportAsync(DateTime startDate, DateTime endDate, int? costCenterId = null)
        {
            var accounts = await _unitOfWork.Accounts.GetQueryable()
                .AsNoTracking()
                .Where(a => a.IsActive && (a.Type == AccountType.Revenue || a.Type == AccountType.Expense))
                .ToListAsync();

            var lineQuery = _unitOfWork.JournalEntryLines.GetQueryable()
                .AsNoTracking()
                .Include(l => l.Account)
                .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted &&
                            l.JournalEntry.Date.Date >= startDate.Date && l.JournalEntry.Date.Date <= endDate.Date &&
                            (l.Account.Type == AccountType.Revenue || l.Account.Type == AccountType.Expense));

            if (costCenterId.HasValue)
            {
                lineQuery = lineQuery.Where(l => l.CostCenterId == costCenterId.Value);
            }

            var groupedLines = await lineQuery
                .GroupBy(l => l.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(l => l.Debit),
                    TotalCredit = g.Sum(l => l.Credit)
                })
                .ToDictionaryAsync(g => g.AccountId, g => g);

            var leafBalances = new Dictionary<int, decimal>();
            foreach (var account in accounts.Where(a => !a.IsGroup))
            {
                if (groupedLines.TryGetValue(account.Id, out var lineTotals))
                {
                    var isDebitNormal = account.Type == AccountType.Expense;
                    var netBalance = isDebitNormal
                        ? lineTotals.TotalDebit - lineTotals.TotalCredit
                        : lineTotals.TotalCredit - lineTotals.TotalDebit;
                    
                    leafBalances[account.Id] = netBalance;
                }
                else
                {
                    leafBalances[account.Id] = 0m;
                }
            }

            var revenues = GetHierarchicalBalances(accounts.Where(a => a.Type == AccountType.Revenue).ToList(), leafBalances, null);
            var expenses = GetHierarchicalBalances(accounts.Where(a => a.Type == AccountType.Expense).ToList(), leafBalances, null);

            var totalRevenue = revenues.Sum(r => r.Balance);
            var totalExpense = expenses.Sum(e => e.Balance);
            var netProfitLoss = totalRevenue - totalExpense;

            var report = new PnLReportDto
            {
                Revenues = revenues,
                Expenses = expenses,
                TotalRevenue = totalRevenue,
                TotalExpense = totalExpense,
                NetProfitLoss = netProfitLoss
            };

            return new ApiResponse<PnLReportDto>(report, "P&L Report generated successfully.");
        }

        public async Task<ApiResponse<BalanceSheetDto>> GetBalanceSheetAsync(DateTime asOfDate)
        {
            var accounts = await _unitOfWork.Accounts.GetQueryable()
                .AsNoTracking()
                .Where(a => a.IsActive && 
                           (a.Type == AccountType.Asset || a.Type == AccountType.Liability || a.Type == AccountType.Equity))
                .ToListAsync();

            var lineQuery = _unitOfWork.JournalEntryLines.GetQueryable()
                .AsNoTracking()
                .Include(l => l.Account)
                .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted &&
                            l.JournalEntry.Date.Date <= asOfDate.Date &&
                            (l.Account.Type == AccountType.Asset || l.Account.Type == AccountType.Liability || l.Account.Type == AccountType.Equity));

            var groupedLines = await lineQuery
                .GroupBy(l => l.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(l => l.Debit),
                    TotalCredit = g.Sum(l => l.Credit)
                })
                .ToDictionaryAsync(g => g.AccountId, g => g);

            var leafBalances = new Dictionary<int, decimal>();
            foreach (var account in accounts.Where(a => !a.IsGroup))
            {
                if (groupedLines.TryGetValue(account.Id, out var lineTotals))
                {
                    var isDebitNormal = account.Type == AccountType.Asset;
                    var netBalance = isDebitNormal
                        ? lineTotals.TotalDebit - lineTotals.TotalCredit
                        : lineTotals.TotalCredit - lineTotals.TotalDebit;
                    
                    leafBalances[account.Id] = netBalance;
                }
                else
                {
                    leafBalances[account.Id] = 0m;
                }
            }

            // Calculate Net Profit up to asOfDate to add to Retained Earnings
            var pnlQuery = _unitOfWork.JournalEntryLines.GetQueryable()
                .AsNoTracking()
                .Include(l => l.Account)
                .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted &&
                            l.JournalEntry.Date.Date <= asOfDate.Date &&
                            (l.Account.Type == AccountType.Revenue || l.Account.Type == AccountType.Expense));

            var pnlLines = await pnlQuery
                .Select(l => new { l.Account.Type, l.Debit, l.Credit })
                .ToListAsync();

            decimal totalRev = pnlLines.Where(l => l.Type == AccountType.Revenue).Sum(l => l.Credit - l.Debit);
            decimal totalExp = pnlLines.Where(l => l.Type == AccountType.Expense).Sum(l => l.Debit - l.Credit);
            // In a real accounting system, Revenues are credits, Expenses are debits.
            // Net income is Credit - Debit. 
            decimal netProfit = totalRev - totalExp;

            // Find Retained Earnings account
            var retainedEarningsAcc = accounts.FirstOrDefault(a => a.Type == AccountType.Equity && !a.IsGroup && a.NameEn.Contains("Retained Earnings", StringComparison.OrdinalIgnoreCase));
            if (retainedEarningsAcc != null)
            {
                if (leafBalances.ContainsKey(retainedEarningsAcc.Id))
                    leafBalances[retainedEarningsAcc.Id] += netProfit;
                else
                    leafBalances[retainedEarningsAcc.Id] = netProfit;
            }

            var assets = GetHierarchicalBalances(accounts.Where(a => a.Type == AccountType.Asset).ToList(), leafBalances, null);
            var liabilities = GetHierarchicalBalances(accounts.Where(a => a.Type == AccountType.Liability).ToList(), leafBalances, null);
            var equities = GetHierarchicalBalances(accounts.Where(a => a.Type == AccountType.Equity).ToList(), leafBalances, null);
            
            // If Retained Earnings wasn't found as a real account, append it as a virtual line to Equity root level.
            if (retainedEarningsAcc == null)
            {
                equities.Add(new FinancialReportLineDto
                {
                    AccountId = 0,
                    AccountCode = "EQ-RE",
                    AccountName = "Retained Earnings (Calculated)",
                    Level = 1,
                    IsGroup = false,
                    Balance = netProfit
                });
            }

            var totalAssets = assets.Sum(a => a.Balance);
            var totalLiabilities = liabilities.Sum(l => l.Balance);
            var totalEquity = equities.Sum(e => e.Balance);

            var report = new BalanceSheetDto
            {
                Assets = assets,
                Liabilities = liabilities,
                Equities = equities,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity
            };

            return new ApiResponse<BalanceSheetDto>(report, "Balance Sheet generated successfully.");
        }

        private List<FinancialReportLineDto> GetHierarchicalBalances(List<Account> allReportAccounts, Dictionary<int, decimal> leafBalances, int? parentAccountId)
        {
            var nodes = allReportAccounts.Where(a => a.ParentAccountId == parentAccountId).ToList();
            var reportLines = new List<FinancialReportLineDto>();

            foreach (var node in nodes)
            {
                var line = new FinancialReportLineDto
                {
                    AccountId = node.Id,
                    AccountCode = node.Code,
                    AccountName = node.NameEn,
                    Level = node.Level,
                    IsGroup = node.IsGroup
                };

                if (node.IsGroup)
                {
                    line.ChildLines = GetHierarchicalBalances(allReportAccounts, leafBalances, node.Id);
                    line.Balance = line.ChildLines.Sum(c => c.Balance);
                }
                else
                {
                    line.Balance = leafBalances.TryGetValue(node.Id, out var bal) ? bal : 0m;
                }

                // Only include if there's a balance or it's a group with children
                if (line.Balance != 0 || (line.IsGroup && line.ChildLines.Any()))
                {
                    reportLines.Add(line);
                }
            }

            return reportLines;
        }
    }
}

