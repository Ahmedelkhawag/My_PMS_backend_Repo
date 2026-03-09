using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Shifts;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
	public class ShiftService : IShiftService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<ShiftService> _logger;
		private readonly IMapper _mapper;
		private readonly IAccountingService _accountingService;

		public ShiftService(IUnitOfWork unitOfWork, ILogger<ShiftService> logger, IMapper mapper, IAccountingService accountingService)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_mapper = mapper;
			_accountingService = accountingService;
		}

        public async Task<ResponseObjectDto<ShiftDto>> OpenShiftAsync(string userId, OpenShiftDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ResponseObjectDto<ShiftDto>.Failure("معرف المستخدم غير صحيح.", 400);
            }

            var hasOpenShift = await _unitOfWork.EmployeeShifts
                .GetQueryable()
                .AnyAsync(s => s.EmployeeId == userId && !s.IsClosed);

            if (hasOpenShift)
            {
                return ResponseObjectDto<ShiftDto>.Failure("المستخدم لديه وردية مفتوحة بالفعل.", 400);
            }

            var shift = new EmployeeShift
            {
                EmployeeId = userId,
                StartedAt = DateTime.UtcNow,
                EndedAt = null,
                StartingCash = dto?.StartingCash ?? 0m,
                SystemCalculatedCash = 0m,
                ActualCashHanded = null,
                Difference = null,
                Notes = string.Empty,
                IsClosed = false
            };

            await _unitOfWork.EmployeeShifts.AddAsync(shift);
            await _unitOfWork.CompleteAsync();

            return ResponseObjectDto<ShiftDto>.Success(_mapper.Map<ShiftDto>(shift), "تم فتح الوردية بنجاح.");
        }

        public async Task<ResponseObjectDto<ShiftReportDto>> GetCurrentShiftStatusAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ResponseObjectDto<ShiftReportDto>.Failure("معرف المستخدم غير صحيح.", 400);
            }

            var shift = await _unitOfWork.EmployeeShifts
                .GetQueryable()
                .AsNoTracking()
                .OrderByDescending(s => s.StartedAt)
                .FirstOrDefaultAsync(s => s.EmployeeId == userId && !s.IsClosed);

            if (shift == null)
            {
                return ResponseObjectDto<ShiftReportDto>.Failure("لا توجد وردية نشطة لهذا المستخدم.", 404);
            }

            var report = await BuildShiftReportAsync(shift.Id, shift.StartedAt);
            return ResponseObjectDto<ShiftReportDto>.Success(report, "تم جلب تقرير الوردية بنجاح.");
        }

        public async Task<ResponseObjectDto<ShiftDto>> CloseShiftAsync(string userId, CloseShiftDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ResponseObjectDto<ShiftDto>.Failure("معرف المستخدم غير صحيح.", 400);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shift = await _unitOfWork.EmployeeShifts
                    .GetQueryable()
                    .OrderByDescending(s => s.StartedAt)
                    .FirstOrDefaultAsync(s => s.EmployeeId == userId && !s.IsClosed);

                if (shift == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ResponseObjectDto<ShiftDto>.Failure("لا توجد وردية مفتوحة لإغلاقها.", 404);
                }

                var report = await BuildShiftReportAsync(shift.Id, shift.StartedAt);
                var netSystemCash = report.NetCash;

                var expectedCash = shift.StartingCash + netSystemCash;
                var actualCash = dto?.ActualCash ?? 0m;
                var difference = actualCash - expectedCash;

                shift.SystemCalculatedCash = netSystemCash;
                shift.ActualCashHanded = actualCash;
                shift.Difference = difference;
                shift.Notes = dto?.Notes ?? string.Empty;
                shift.IsClosed = true;
                shift.EndedAt = DateTime.UtcNow;

                _unitOfWork.EmployeeShifts.Update(shift);
                await _unitOfWork.CompleteAsync();

                if (difference != 0)
                {
                    var businessDate = await _unitOfWork.GetCurrentBusinessDateAsync();
                    var transactionType = difference > 0 ? TransactionType.ShiftOverage : TransactionType.ShiftShortage;

                    var reconciliationTx = new FolioTransaction
                    {
                        FolioId = null, // Discrepancy does not belong to a specific guest
                        Date = DateTime.UtcNow,
                        BusinessDate = businessDate,
                        Type = transactionType,
                        Amount = Math.Abs(difference),
                        Description = $"Shift Reconciliation Adjustment for Shift #{shift.Id}",
                        ReferenceNo = $"SHIFT-{shift.Id}",
                        ShiftId = shift.Id,
                        IsVoided = false
                    };

                    await _unitOfWork.FolioTransactions.AddAsync(reconciliationTx);
                    await _unitOfWork.CompleteAsync(); // Get Transaction ID

                    shift.ReconciliationTransactionId = reconciliationTx.Id;
                    _unitOfWork.EmployeeShifts.Update(shift);
                    await _unitOfWork.CompleteAsync();

                    var glResult = await _accountingService.PostTransactionToGLAsync(reconciliationTx.Id);
                    if (!glResult.Succeeded)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ResponseObjectDto<ShiftDto>.Failure($"فشل في ترحيل تسوية الخزنة للقيود اليومية: {glResult.Message}", 500);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                return ResponseObjectDto<ShiftDto>.Success(_mapper.Map<ShiftDto>(shift), "تم إغلاق الوردية وتمرير التسويات بنجاح.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while closing shift for user {UserId}", userId);
                return ResponseObjectDto<ShiftDto>.Failure("حدث خطأ أثناء إغلاق الوردية.", 500);
            }
        }

        public async Task<ResponseObjectDto<IEnumerable<ShiftDto>>> GetShiftHistoryAsync(UserFilterDto filter)
        {
            filter ??= new UserFilterDto();

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

            var query = _unitOfWork.EmployeeShifts
                .GetQueryable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(s => s.EmployeeId.Contains(search));
            }

            if (filter.IsActive.HasValue)
            {
                query = filter.IsActive.Value
                    ? query.Where(s => !s.IsClosed)
                    : query.Where(s => s.IsClosed);
            }

            if (filter.ShowOnlyDiscrepancies == true)
            {
                query = query.Where(s => s.IsClosed && s.Difference.HasValue && s.Difference.Value != 0m);
            }

            var shifts = await query
                .OrderByDescending(s => s.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<ShiftDto>>(shifts);
            return ResponseObjectDto<IEnumerable<ShiftDto>>.Success(dtos, "تم جلب تاريخ الورديات بنجاح.");
        }

        public async Task AutoCloseExpiredShiftsAsync()
        {
            var cutoff = DateTime.UtcNow.AddHours(-12);

            var expiredShifts = await _unitOfWork.EmployeeShifts
                .GetQueryable()
                .Where(s => !s.IsClosed && s.StartedAt <= cutoff)
                .ToListAsync();

            if (expiredShifts.Count == 0)
            {
                _logger.LogInformation("AutoCloseExpiredShifts: No expired shifts found.");
                return;
            }

            foreach (var shift in expiredShifts)
            {
                var report = await BuildShiftReportAsync(shift.Id, shift.StartedAt);
                var netCash = report.NetCash;

                shift.SystemCalculatedCash = netCash;
                shift.ActualCashHanded = netCash;
                shift.Difference = 0m;
                shift.IsClosed = true;
                shift.EndedAt = DateTime.UtcNow;
                shift.Notes = "System Auto-Closure: Shift exceeded 12 hours limit.";

                _unitOfWork.EmployeeShifts.Update(shift);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("AutoCloseExpiredShifts: {Count} shift(s) auto-closed.", expiredShifts.Count);
        }

        private async Task<ShiftReportDto> BuildShiftReportAsync(int shiftId, DateTime shiftStartTimeUtc)
        {
            var txQuery = _unitOfWork.FolioTransactions
                .GetQueryable()
                .AsNoTracking()
                .Where(t => t.ShiftId == shiftId && !t.IsVoided);

            var cashPayments = await txQuery
                .Where(t => t.Type == TransactionType.CashPayment && t.Amount > 0m)
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var cashRefunds = await txQuery
                .Where(t => t.Type == TransactionType.CashPayment && t.Amount < 0m)
                .SumAsync(t => (decimal?)(-t.Amount)) ?? 0m;

            var visaPayments = await txQuery
                .Where(t =>
                    (t.Type == TransactionType.CardPayment || t.Type == TransactionType.BankTransferPayment) &&
                    t.Amount > 0m)
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var visaRefunds = await txQuery
                .Where(t =>
                    (t.Type == TransactionType.CardPayment || t.Type == TransactionType.BankTransferPayment) &&
                    t.Amount < 0m)
                .SumAsync(t => (decimal?)(-t.Amount)) ?? 0m;

            var netCash = cashPayments - cashRefunds;

            return new ShiftReportDto
            {
                StartTime = shiftStartTimeUtc,
                TotalCashPayments = cashPayments,
                TotalCashRefunds = cashRefunds,
                TotalVisaPayments = visaPayments,
                TotalVisaRefunds = visaRefunds,
                NetCash = netCash
            };
        }
    }
}
