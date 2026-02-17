using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Configuration;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class ConfigurationService: IConfigurationService
	{
		private readonly IUnitOfWork _unitOfWork; // 👈 الحقن هنا

		public ConfigurationService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public Task<ResponseObjectDto<StatusConfigurationDto>> GetStatusConfigurationAsync()
		{
			var hkColors = new Dictionary<HKStatus, string>
			{
				{ HKStatus.Clean, "#28a745" },
				{ HKStatus.Dirty, "#dc3545" },
				{ HKStatus.Inspected, "#17a2b8" },
				{ HKStatus.OOO, "#343a40" },
				{ HKStatus.OOS, "#6c757d" }
			};
			var foColors = new Dictionary<FOStatus, string>
			{
				{ FOStatus.Vacant, "#28a745" },
				{ FOStatus.Occupied, "#007bff" }
			};
			const string bedTypeColor = "#000000";

			var hkStatuses = Enum.GetValues(typeof(HKStatus)).Cast<HKStatus>()
				.Select(v => new EnumLookupDto { Value = (int)v, Name = v.ToString(), ColorCode = hkColors[v] })
				.ToList();
			var foStatuses = Enum.GetValues(typeof(FOStatus)).Cast<FOStatus>()
				.Select(v => new EnumLookupDto { Value = (int)v, Name = v.ToString(), ColorCode = foColors[v] })
				.ToList();
			var bedTypes = Enum.GetValues(typeof(BedType)).Cast<BedType>()
				.Select(v => new EnumLookupDto { Value = (int)v, Name = v.ToString(), ColorCode = bedTypeColor })
				.ToList();

			var data = new StatusConfigurationDto
			{
				HkStatuses = hkStatuses,
				FoStatuses = foStatuses,
				BedTypes = bedTypes
			};

			var response = new ResponseObjectDto<StatusConfigurationDto>
			{
				IsSuccess = true,
				StatusCode = 200,
				Message = "Status configuration retrieved successfully",
				Data = data
			};

			return Task.FromResult(response);
		}

		public async Task<IEnumerable<BookingSourceLookupDto>> GetBookingSourcesAsync()
		{
			// بنستخدم GetQueryable عشان نعمل Projection (Select) في الداتابيز
			return await _unitOfWork.BookingSources.GetQueryable()
				.Where(x => x.IsActive)
				.Select(x => new BookingSourceLookupDto
				{
					Id = x.Id,
					Name = x.Name,
					RequiresExternalReference = x.RequiresExternalReference
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<LookupDto>> GetMarketSegmentsAsync()
		{
			return await _unitOfWork.MarketSegments.GetQueryable()
				.Where(x => x.IsActive)
				.Select(x => new LookupDto { Id = x.Id, Name = x.Name })
				.ToListAsync();
		}

		public async Task<IEnumerable<MealPlanLookupDto>> GetMealPlansAsync()
		{
			return await _unitOfWork.MealPlans.GetQueryable()
				.Where(x => x.IsActive)
				.Select(x => new MealPlanLookupDto
				{
					Id = x.Id,
					Name = x.Name,
					Price = x.Price
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<RoomStatusLookupDto>> GetRoomStatusesAsync()
		{
			return await _unitOfWork.RoomStatuses.GetQueryable()
				.Where(x => x.IsActive)
				.Select(x => new RoomStatusLookupDto
				{
					Id = x.Id,
					Name = x.Name,
					Color = x.Color
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<LookupDto>> GetRoomTypesLookupAsync()
		{
			return await _unitOfWork.RoomTypes.GetQueryable()
			   .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
			   .ToListAsync();
		}


		public async Task<IEnumerable<ExtraServiceLookupDto>> GetExtraServicesAsync()
		{
			return await _unitOfWork.ExtraServices.GetQueryable()
				.Where(x => x.IsActive)
				.Select(x => new ExtraServiceLookupDto
				{
					Id = x.Id,
					Name = x.Name,
					Price = x.Price,
					IsPerDay = x.IsPerDay
				})
				.ToListAsync();
		}

		public Task<IEnumerable<LookupDto>> GetReservationStatusesAsync()
		{
			var values = Enum.GetValues(typeof(ReservationStatus))
				.Cast<ReservationStatus>()
				.Select(v => new LookupDto
				{
					Id = (int)v,
					Name = v.ToString()
				})
				.ToList()
				.AsEnumerable();

			return Task.FromResult(values);
		}

		public Task<ResponseObjectDto<List<EnumLookupDto>>> GetTransactionTypesLookupAsync()
		{
			var values = Enum.GetValues(typeof(TransactionType))
				.Cast<TransactionType>()
				.Select(v => new EnumLookupDto
				{
					Value = (int)v,
					Name = v.ToString(),
					ColorCode = string.Empty
				})
				.ToList();

			var response = new ResponseObjectDto<List<EnumLookupDto>>
			{
				IsSuccess = true,
				StatusCode = 200,
				Message = "Transaction types retrieved successfully",
				Data = values
			};

			return Task.FromResult(response);
		}

		public async Task<ResponseObjectDto<AppLookupsDto>> GetAllLookupsAsync()
		{
			// NOTE:
			// We call the existing async lookup methods sequentially to avoid
			// running multiple EF Core queries concurrently on the same DbContext.

			var roomTypes = (await GetRoomTypesLookupAsync()).ToList();
			var bookingSources = (await GetBookingSourcesAsync()).ToList();
			var marketSegments = (await GetMarketSegmentsAsync()).ToList();
			var mealPlans = (await GetMealPlansAsync()).ToList();
			var extraServices = (await GetExtraServicesAsync()).ToList();
			var reservationStatuses = (await GetReservationStatusesAsync()).ToList();

			var transactionTypesResult = await GetTransactionTypesLookupAsync();
			if (!transactionTypesResult.IsSuccess || transactionTypesResult.Data == null)
			{
				return new ResponseObjectDto<AppLookupsDto>
				{
					IsSuccess = false,
					StatusCode = transactionTypesResult.StatusCode > 0 ? transactionTypesResult.StatusCode : 500,
					Message = transactionTypesResult.Message ?? "Failed to load transaction types",
					Data = null
				};
			}

			// Generate enum-based status lookups with color codes
			var hkColors = new Dictionary<HKStatus, string>
			{
				{ HKStatus.Clean, "#43A047" },
				{ HKStatus.Dirty, "#E53935" },
				{ HKStatus.Inspected, "#00ACC1" },
				{ HKStatus.OOO, "#263238" },
				{ HKStatus.OOS, "#78909C" }
			};
			var foColors = new Dictionary<FOStatus, string>
			{
				{ FOStatus.Vacant, "#81C784" },
				{ FOStatus.Occupied, "#1E88E5" }
			};
			const string bedTypeColor = "#000000";

			var hkStatuses = Enum.GetValues(typeof(HKStatus)).Cast<HKStatus>()
				.Select(v => new EnumLookupDto { Value = (int)v, Name = v.ToString(), ColorCode = hkColors[v] })
				.ToList();
			var foStatuses = Enum.GetValues(typeof(FOStatus)).Cast<FOStatus>()
				.Select(v => new EnumLookupDto { Value = (int)v, Name = v.ToString(), ColorCode = foColors[v] })
				.ToList();
			var bedTypes = Enum.GetValues(typeof(BedType)).Cast<BedType>()
				.Select(v => new EnumLookupDto { Value = (int)v, Name = v.ToString(), ColorCode = bedTypeColor })
				.ToList();

			var lookups = new AppLookupsDto
			{
				RoomTypes = roomTypes,
				BookingSources = bookingSources,
				MarketSegments = marketSegments,
				MealPlans = mealPlans,
				ExtraServices = extraServices,
				HkStatuses = hkStatuses,
				FoStatuses = foStatuses,
				BedTypes = bedTypes,
				TransactionTypes = transactionTypesResult.Data,
				ReservationStatuses = reservationStatuses
			};

			return new ResponseObjectDto<AppLookupsDto>
			{
				IsSuccess = true,
				StatusCode = 200,
				Message = "All lookups retrieved successfully",
				Data = lookups
			};
		}
	}
}
