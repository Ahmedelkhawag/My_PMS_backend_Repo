using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Configuration;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using System;
using System.Collections.Generic;
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

		public async Task<IEnumerable<LookupDto>> GetBookingSourcesAsync()
		{
			// بنستخدم GetQueryable عشان نعمل Projection (Select) في الداتابيز
			return await _unitOfWork.BookingSources.GetQueryable()
				.Where(x => x.IsActive)
				.Select(x => new LookupDto { Id = x.Id, Name = x.Name })
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
	}
}
