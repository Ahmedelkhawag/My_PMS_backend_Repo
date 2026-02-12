using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Rooms;
using PMS.Application.Interfaces.Services;

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
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roomService.CreateRoomAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            if (dto == null)
                return BadRequest("يجب إرسال حقل واحد على الأقل للتحديث");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roomService.UpdateRoomAsync(id, dto);

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
            var result = await _roomService.DeleteRoomAsync(id);

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
            var result = await _roomService.RestoreRoomAsync(id);

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
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int statusId, [FromQuery] string? notes)
        {
            var result = await _roomService.ChangeRoomStatusAsync(id, statusId, notes);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<RoomDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _roomService.GetRoomByIdAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }
    }
}
