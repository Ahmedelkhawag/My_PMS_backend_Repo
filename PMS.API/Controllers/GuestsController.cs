using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
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
                return StatusCode(result.StatusCode, result);

            return StatusCode(201, result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<GuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var result = await _guestService.GetAllGuestsAsync(search);
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
            if (id != dto.Id)
                return BadRequest("رقم المعرف غير متطابق");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _guestService.UpdateGuestAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

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
                return StatusCode(result.StatusCode, result);

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
