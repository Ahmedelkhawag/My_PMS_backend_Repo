using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.BackOffice;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.BackOffice;
using PMS.Domain.Enums.BackOffice;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == folioTransactionId);

            if (transaction == null)
            {
                return new ApiResponse<bool>("Folio transaction not found.");
            }

            if (transaction.IsVoided)
            {
                return new ApiResponse<bool>("Cannot post a voided transaction.");
            }

            var mapping = await _unitOfWork.JournalEntryMappings
                .FindAsync(m => m.TransactionType == transaction.Type && m.IsActive);

            if (mapping == null)
            {
                return new ApiResponse<bool>($"No journal entry mapping configured for transaction type '{transaction.Type}'.");
            }

            var debitAccount = await _unitOfWork.Accounts.GetByIdAsync(mapping.DebitAccountId);
            var creditAccount = await _unitOfWork.Accounts.GetByIdAsync(mapping.CreditAccountId);

            if (debitAccount == null || creditAccount == null)
            {
                return new ApiResponse<bool>("Mapped debit or credit account not found.");
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
                BusinessDayId = businessDay.Id
            };

            var debitLine = new JournalEntryLine
            {
                AccountId = mapping.DebitAccountId,
                Debit = absAmount,
                Credit = 0m,
                Memo = transaction.Description
            };

            var creditLine = new JournalEntryLine
            {
                AccountId = mapping.CreditAccountId,
                Debit = 0m,
                Credit = absAmount,
                Memo = transaction.Description
            };

            journalEntry.Lines.Add(debitLine);
            journalEntry.Lines.Add(creditLine);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                ApplyBalanceEffect(debitAccount, absAmount, isDebit: true);
                ApplyBalanceEffect(creditAccount, absAmount, isDebit: false);

                _unitOfWork.Accounts.Update(debitAccount);
                _unitOfWork.Accounts.Update(creditAccount);

                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "Transaction posted to GL successfully.");
            }
            catch
            {
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

            var totalDebit = dto.Lines.Sum(l => l.Debit);
            var totalCredit = dto.Lines.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
            {
                return new ApiResponse<bool>("Journal entry is not balanced. Total debit must equal total credit.");
            }

            var accountIds = dto.Lines.Select(l => l.AccountId).Distinct().ToList();
            var accounts = await _unitOfWork.Accounts.GetQueryable()
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();

            if (accounts.Count != accountIds.Count)
            {
                return new ApiResponse<bool>("One or more accounts do not exist.");
            }

            if (accounts.Any(a => a.IsGroup))
            {
                return new ApiResponse<bool>("Journal entries can only be posted to non-group (leaf) accounts.");
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

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.JournalEntries.AddAsync(journalEntry);
                await _unitOfWork.JournalEntryLines.AddRangeAsync(journalEntry.Lines);

                foreach (var line in journalEntry.Lines)
                {
                    var account = accounts.First(a => a.Id == line.AccountId);
                    var amount = line.Debit != 0 ? line.Debit : line.Credit;
                    var isDebit = line.Debit > 0;
                    ApplyBalanceEffect(account, amount, isDebit);
                    _unitOfWork.Accounts.Update(account);
                }

                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>(true, "Manual journal entry created successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
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

            foreach (var account in accounts)
            {
                var (debit, credit) = GetTrialBalanceDebitCredit(account);
                items.Add(new TrialBalanceItemDto(
                    account.Code,
                    account.NameEn,
                    debit,
                    credit
                ));
                totalDebit += debit;
                totalCredit += credit;
            }

            var isBalanced = totalDebit == totalCredit;
            var report = new TrialBalanceReportDto(items, totalDebit, totalCredit, isBalanced);

            return new ApiResponse<TrialBalanceReportDto>(report, "Trial balance retrieved successfully.");
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
                .Where(l => l.AccountId == accountId);

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

    }
}

