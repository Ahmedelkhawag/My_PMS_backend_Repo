using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsConfigurationController : ControllerBase
    {
		private readonly IConfigurationService _configService;

		public LookupsConfigurationController(IConfigurationService configService)
		{
			_configService = configService;
		}

		[HttpGet("sources")]
		public async Task<IActionResult> GetSources()
		{
			return Ok(await _configService.GetBookingSourcesAsync());
		}

		[HttpGet("markets")]
		public async Task<IActionResult> GetMarkets()
		{
			return Ok(await _configService.GetMarketSegmentsAsync());
		}

		[HttpGet("meal-plans")]
		public async Task<IActionResult> GetMealPlans()
		{
			return Ok(await _configService.GetMealPlansAsync());
		}

		[HttpGet("room-statuses")]
		public async Task<IActionResult> GetRoomStatuses()
		{
			return Ok(await _configService.GetRoomStatusesAsync());
		}

		[HttpGet("room-types")]
		public async Task<IActionResult> GetRoomTypes()
		{
			return Ok(await _configService.GetRoomTypesLookupAsync());
		}


		[HttpGet("extra-services")]
		public async Task<IActionResult> GetExtraServices()
		{
			return Ok(await _configService.GetExtraServicesAsync());
		}
	}
}
