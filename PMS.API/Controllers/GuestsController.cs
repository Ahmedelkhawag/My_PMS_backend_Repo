using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Guests;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/guests")]
    [ApiController]
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _guestService;

        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateGuestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _guestService.AddGuestAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(201, result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<PagedResult<GuestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _guestService.GetAllGuestsAsync(search, pageNumber, pageSize);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

		[HttpGet("{id}")]
		[Authorize]
		[ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _guestService.GetGuestByIdAsync(id);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
			return Ok(result);
		}

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestDto dto)
        {
            if (dto == null)
                return BadRequest("يجب إرسال حقل واحد على الأقل للتحديث");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _guestService.UpdateGuestAsync(id, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _guestService.DeleteGuestAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPut("{id}/restore")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _guestService.RestoreGuestAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpGet("summary")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _guestService.GetGuestStatsAsync();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        //[HttpGet("search")]
        //public async Task<IActionResult> Search([FromQuery] string term)
        //{
        //    var result = await _guestService.SearchGuestsAsync(term);
        //    return StatusCode(result.StatusCode, result);
        //}
    }
}
