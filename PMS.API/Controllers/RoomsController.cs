using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Rooms;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: api/rooms?floor=1&status=Available&type=2
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? floor, [FromQuery] int? type, [FromQuery] string? status)
        {
            var result = await _roomService.GetAllRoomsAsync(floor, type, status);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roomService.CreateRoomAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("Update-Room/{id}")]
        [Authorize]

        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            if (id != dto.Id)
                return BadRequest("رقم المعرف غير متطابق");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roomService.UpdateRoomAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpDelete("delete-room/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
