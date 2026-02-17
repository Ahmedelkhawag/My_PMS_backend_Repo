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
		/// Opens a new shift for the current logged-in user.
		/// The user can only have one active (not closed) shift at a time.
		/// </summary>
		[HttpPost("open")]
		[Authorize]
		[ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new ApiResponse<ShiftDto>("Invalid request."));
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrWhiteSpace(userId))
			{
				return Unauthorized(new ApiResponse<ShiftDto>("Unauthorized: cannot determine current user."));
			}

			var result = await _shiftService.OpenShiftAsync(userId, dto);
			if (!result.Succeeded)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		/// <summary>
		/// Returns the current shift report for the logged-in user.
		/// </summary>
		[HttpGet("current")]
		[Authorize]
		[ProducesResponseType(typeof(ApiResponse<ShiftReportDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<ShiftReportDto>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<ShiftReportDto>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> GetCurrentShiftReport()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrWhiteSpace(userId))
			{
				return Unauthorized(new ApiResponse<ShiftReportDto>("Unauthorized: cannot determine current user."));
			}

			var result = await _shiftService.GetCurrentShiftStatusAsync(userId);
			if (!result.Succeeded)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		/// <summary>
		/// Closes the current active shift for the logged-in user.
		/// </summary>
		[HttpPost("close")]
		[Authorize]
		[ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> CloseShift([FromBody] CloseShiftDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new ApiResponse<ShiftDto>("Invalid request."));
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrWhiteSpace(userId))
			{
				return Unauthorized(new ApiResponse<ShiftDto>("Unauthorized: cannot determine current user."));
			}

			var result = await _shiftService.CloseShiftAsync(userId, dto);
			if (!result.Succeeded)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		/// <summary>
		/// Returns shift history. Admin only.
		/// </summary>
		[HttpGet("history")]
		[Authorize(Roles = $"{Roles.HotelManager},{Roles.SuperAdmin}")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDto>>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDto>>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDto>>), StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> GetShiftHistory([FromQuery] UserFilterDto filter)
		{
			var result = await _shiftService.GetShiftHistoryAsync(filter);
			if (!result.Succeeded)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}
	}
}

