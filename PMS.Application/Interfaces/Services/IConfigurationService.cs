using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IConfigurationService
    {
		Task<ResponseObjectDto<StatusConfigurationDto>> GetStatusConfigurationAsync();
		Task<IEnumerable<LookupDto>> GetBookingSourcesAsync();
		Task<IEnumerable<LookupDto>> GetMarketSegmentsAsync();
		Task<IEnumerable<MealPlanLookupDto>> GetMealPlansAsync();
		Task<IEnumerable<RoomStatusLookupDto>> GetRoomStatusesAsync();
		Task<IEnumerable<LookupDto>> GetRoomTypesLookupAsync();
		Task<IEnumerable<ExtraServiceLookupDto>> GetExtraServicesAsync();
		Task<IEnumerable<LookupDto>> GetReservationStatusesAsync();
	}
}
