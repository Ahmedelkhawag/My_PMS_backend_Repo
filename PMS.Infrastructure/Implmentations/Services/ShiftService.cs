using Microsoft.EntityFrameworkCore;
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

		public ShiftService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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

            return ResponseObjectDto<ShiftDto>.Success(MapToShiftDto(shift), "تم فتح الوردية بنجاح.");
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

            var shift = await _unitOfWork.EmployeeShifts
                .GetQueryable()
                .OrderByDescending(s => s.StartedAt)
                .FirstOrDefaultAsync(s => s.EmployeeId == userId && !s.IsClosed);

            if (shift == null)
            {
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

            return ResponseObjectDto<ShiftDto>.Success(MapToShiftDto(shift), "تم إغلاق الوردية بنجاح.");
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

            var dtos = shifts.Select(MapToShiftDto).ToList();
            return ResponseObjectDto<IEnumerable<ShiftDto>>.Success(dtos, "تم جلب تاريخ الورديات بنجاح.");
        }

        // الدوال المساعدة (BuildShiftReportAsync و MapToShiftDto) بتفضل زي ما هي لإنها داخلية
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

        private static ShiftDto MapToShiftDto(EmployeeShift shift)
        {
            return new ShiftDto
            {
                Id = shift.Id,
                EmployeeId = shift.EmployeeId,
                StartedAt = shift.StartedAt,
                EndedAt = shift.EndedAt,
                StartingCash = shift.StartingCash,
                SystemCalculatedCash = shift.SystemCalculatedCash,
                ActualCashHanded = shift.ActualCashHanded,
                Difference = shift.Difference,
                Notes = shift.Notes ?? string.Empty,
                IsClosed = shift.IsClosed,
                CreatedBy = shift.CreatedBy,
                CreatedAt = shift.CreatedAt,
                UpdatedBy = shift.LastModifiedBy,
                UpdatedAt = shift.LastModifiedAt
            };
        }
    }
}

