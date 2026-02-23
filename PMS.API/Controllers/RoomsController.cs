using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Dashboard;
using PMS.Application.DTOs.Rooms;
using PMS.Application.Interfaces.Services;
using PMS.Domain.Enums;

namespace PMS.API.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<PagedResult<RoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? floor,
            [FromQuery] int? type,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _roomService.GetAllRoomsAsync(floor, type, status, pageNumber, pageSize);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات الغرفة غير صالحة", 400));

            var result = await _roomService.CreateRoomAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(201, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            if (dto == null)
                return BadRequest(ResponseObjectDto<string>.Failure("يجب إرسال حقل واحد على الأقل للتحديث", 400));

            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("البيانات المبعوتة غير صالحة", 400));

            var result = await _roomService.UpdateRoomAsync(id, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 404, result);

            return Ok(result);
        }

        [HttpPut("{id}/restore")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _roomService.RestoreRoomAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 404, result);

            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeStatus(
            int id,
            [FromQuery] int statusId,
            [FromQuery] string statusType,
            [FromQuery] string? notes)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                return BadRequest(ResponseObjectDto<string>.Failure("نوع الحالة (statusType) مطلوب.", 400));

            RoomStatusType parsedType;
            switch (statusType.Trim().ToLowerInvariant())
            {
                case "hk":
                    parsedType = RoomStatusType.HouseKeeping;
                    break;
                case "fk":
                case "fo":
                    parsedType = RoomStatusType.FrontOffice;
                    break;
                default:
                    return BadRequest(ResponseObjectDto<string>.Failure("نوع الحالة غير صحيح.", 400));
            }

            var dto = new ChangeRoomStatusDto
            {
                StatusType = parsedType,
                StatusId = statusId,
                Notes = notes
            };

            var result = await _roomService.ChangeRoomStatusAsync(id, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _roomService.GetRoomByIdAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 404, result);

            return Ok(result);
        }

        [HttpGet("summary")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _roomService.GetRoomStatsAsync();

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPost("{id}/maintenance/start")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartMaintenance(int id, [FromBody] RoomMaintenanceDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(ResponseObjectDto<string>.Failure("سبب الصيانة مطلوب.", 400));

            var result = await _roomService.StartMaintenanceAsync(id, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPost("{id}/maintenance/finish")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FinishMaintenance(int id)
        {
            var result = await _roomService.FinishMaintenanceAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }
    }
}
