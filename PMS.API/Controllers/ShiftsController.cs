using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Shifts;
using PMS.Application.Interfaces.Services;
using PMS.Domain.Constants;
using System.Security.Claims;

namespace PMS.API.Controllers
{
	[Route("api/shifts")]
	[ApiController]
	public class ShiftsController : ControllerBase
	{
		private readonly IShiftService _shiftService;

		public ShiftsController(IShiftService shiftService)
		{
			_shiftService = shiftService;
		}

        /// <summary>
        /// فتح وردية جديدة للمستخدم الحالي.
        /// </summary>
        [HttpPost("open")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ShiftDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات فتح الوردية غير صحيحة.", 400));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ResponseObjectDto<string>.Failure("لم يتم التعرف على المستخدم الحالي.", 401));

            // بنفترض إن السيرفيس اتعدلت وبقت ترجع ResponseObjectDto
            var result = await _shiftService.OpenShiftAsync(userId, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        /// <summary>
        /// الحصول على تقرير الوردية الحالية للمستخدم.
        /// </summary>
        [HttpGet("current")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ShiftReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurrentShiftReport()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ResponseObjectDto<string>.Failure("لم يتم التعرف على المستخدم الحالي.", 401));

            var result = await _shiftService.GetCurrentShiftStatusAsync(userId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        /// <summary>
        /// إغلاق الوردية الحالية المفتوحة للمستخدم.
        /// </summary>
        [HttpPost("close")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<ShiftDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CloseShift([FromBody] CloseShiftDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات إغلاق الوردية غير صحيحة.", 400));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ResponseObjectDto<string>.Failure("لم يتم التعرف على المستخدم الحالي.", 401));

            var result = await _shiftService.CloseShiftAsync(userId, dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        /// <summary>
        /// عرض تاريخ الورديات (للمديرين فقط).
        /// </summary>
        [HttpGet("history")]
        [Authorize(Roles = $"{Roles.HotelManager},{Roles.SuperAdmin}")]
        [ProducesResponseType(typeof(ResponseObjectDto<IEnumerable<ShiftDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetShiftHistory([FromQuery] UserFilterDto filter)
        {
            var result = await _shiftService.GetShiftHistoryAsync(filter);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }
    }
}

