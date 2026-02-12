using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _reservationService.CreateReservationAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(201, result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<PagedResult<ReservationListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reservationService.GetAllReservationsAsync(search, status, pageNumber, pageSize);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] SetReservationStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var changeDto = new ChangeReservationStatusDto
            {
                ReservationId = id,
                NewStatus = dto.NewStatus,
                RoomId = dto.RoomId,
                Note = dto.Note
            };
            var result = await _reservationService.ChangeStatusAsync(changeDto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _reservationService.GetReservationByIdAsync(id);
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
            var result = await _reservationService.DeleteReservationAsync(id);
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
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _reservationService.RestoreReservationAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            if (id != dto.Id)
                return BadRequest("رقم المعرف غير متطابق");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _reservationService.UpdateReservationAsync(dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }
    }
}
