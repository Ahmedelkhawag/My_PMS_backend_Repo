using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Reservations;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IRegistrationCardPdfService _registrationCardPdfService;
        private readonly IAuthService _authService;

        public ReservationsController(
            IReservationService reservationService,
            IRegistrationCardPdfService registrationCardPdfService,
            IAuthService authService)
        {
            _reservationService = reservationService;
            _registrationCardPdfService = registrationCardPdfService;
            _authService = authService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات الحجز غير صحيحة", 400));

            var result = await _reservationService.CreateReservationAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(201, result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<PagedResult<ReservationListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] SetReservationStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات الحالة غير صحيحة", 400));

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

        /// <summary>
        /// Generates the bilingual (Arabic/English) Guest Registration Card PDF for the reservation.
        /// </summary>
        [HttpGet("{id}/registration-card")]
        [Authorize]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistrationCard(int id)
        {
            string? receptionistName = "—";
            var profileResult = await _authService.GetCurrentUserProfileAsync();
            if (profileResult.Succeeded && profileResult.Data != null)
                receptionistName = profileResult.Data.FullName;

            var result = await _registrationCardPdfService.GenerateRegistrationCardAsync(id, receptionistName);

            if (result == null)
                return NotFound(ResponseObjectDto<string>.Failure("Reservation not found.", 404));

            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _reservationService.GetReservationByIdAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 404, result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _reservationService.DeleteReservationAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 404, result);

            return Ok(result);
        }

        [HttpPut("{id}/restore")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات التحديث غير صالحة", 400));

            var result = await _reservationService.UpdateReservationAsync(id, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpGet("summary")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ReservationStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _reservationService.GetReservationStatsAsync();

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }
    }
}
