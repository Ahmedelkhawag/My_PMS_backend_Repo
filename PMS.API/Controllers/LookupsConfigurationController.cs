using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Configuration;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/lookups")]
    [ApiController]
    public class LookupsConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configService;

        public LookupsConfigurationController(IConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet("sources")]
        [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSources()
        {
            return Ok(await _configService.GetBookingSourcesAsync());
        }

        [HttpGet("markets")]
        [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMarkets()
        {
            return Ok(await _configService.GetMarketSegmentsAsync());
        }

        [HttpGet("meal-plans")]
        [ProducesResponseType(typeof(IEnumerable<MealPlanLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMealPlans()
        {
            return Ok(await _configService.GetMealPlansAsync());
        }

        [HttpGet("room-statuses")]
        [ProducesResponseType(typeof(IEnumerable<RoomStatusLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoomStatuses()
        {
            return Ok(await _configService.GetRoomStatusesAsync());
        }

        [HttpGet("room-types")]
        [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoomTypes()
        {
            return Ok(await _configService.GetRoomTypesLookupAsync());
        }

        [HttpGet("extra-services")]
        [ProducesResponseType(typeof(IEnumerable<ExtraServiceLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExtraServices()
        {
            return Ok(await _configService.GetExtraServicesAsync());
        }

        [HttpGet("reservation-statuses")]
        [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservationStatuses()
        {
            return Ok(await _configService.GetReservationStatusesAsync());
        }

        [HttpGet("status-configuration")]
        [ProducesResponseType(typeof(ResponseObjectDto<StatusConfigurationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatusConfiguration()
        {
            var result = await _configService.GetStatusConfigurationAsync();
            return Ok(result);
        }

		[HttpGet("transaction-types")]
		[ProducesResponseType(typeof(ResponseObjectDto<List<EnumLookupDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetTransactionTypes()
		{
			var result = await _configService.GetTransactionTypesLookupAsync();
			return Ok(result);
		}

		[HttpGet("all")]
		[ProducesResponseType(typeof(ResponseObjectDto<AppLookupsDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllLookups()
		{
			var result = await _configService.GetAllLookupsAsync();
			return Ok(result);
		}
    }
}
