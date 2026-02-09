using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Guests;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
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
		
		public async Task<IActionResult> GetAll([FromQuery] string? search)
		{
			var result = await _guestService.GetAllGuestsAsync(search);
			return Ok(result);
		}

		[HttpPut("{id}")]
		[Authorize]

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
		//	var result = await _guestService.SearchGuestsAsync(term);
		//	return StatusCode(result.StatusCode, result);
		//}
	}
}
