using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
		private readonly IReservationService _reservationService;

		public ReservationsController(IReservationService reservationService)
		{
			_reservationService = reservationService;
		}

		[HttpPost("creat-reservation")]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _reservationService.CreateReservationAsync(dto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result);

			// بنرجع 201 Created مع بيانات الحجز اللي اتعمل
			return StatusCode(201, result);
		}

		[HttpGet("get-all-reservations")]
		[Authorize]
		public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
		{
			var result = await _reservationService.GetAllReservationsAsync(search, status);

			
			return StatusCode(result.StatusCode, result);
		}

		[HttpPut("change-reservation-status")]
		[Authorize]
		public async Task<IActionResult> ChangeStatus([FromBody] ChangeReservationStatusDto dto)
		{
			var result = await _reservationService.ChangeStatusAsync(dto);
			return StatusCode(result.StatusCode, result);
		}


		[HttpGet("get-reservation-details{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _reservationService.GetReservationByIdAsync(id);
			return StatusCode(result.StatusCode, result);
		}

		[HttpDelete("delete-reservation/{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _reservationService.DeleteReservationAsync(id);
			return StatusCode(result.StatusCode, result);
		}


		[HttpPut("restore-reservation/{id}")]
		[Authorize]
		public async Task<IActionResult> Restore(int id)
		{
			var result = await _reservationService.RestoreReservationAsync(id);
			return StatusCode(result.StatusCode, result);
		}


		[HttpPut("update-reservation")]
		public async Task<IActionResult> Update([FromBody] UpdateReservationDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _reservationService.UpdateReservationAsync(dto);
			return StatusCode(result.StatusCode, result);
		}
	}
}
