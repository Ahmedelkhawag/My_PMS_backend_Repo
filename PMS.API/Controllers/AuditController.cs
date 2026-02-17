using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Audit;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/audit")]
    [ApiController]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly INightAuditService _nightAuditService;

        public AuditController(INightAuditService nightAuditService)
        {
            _nightAuditService = nightAuditService;
        }

        /// <summary>
        /// Returns the current business date and audit status.
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<AuditStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuditStatusDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStatus()
        {
            var result = await _nightAuditService.GetCurrentStatusAsync();
            return Ok(result);
        }

        /// <summary>
        /// Runs the night audit and rolls the business date forward.
        /// This operation is restricted to Admin roles.
        /// </summary>
        [HttpPost("run")]
        [Authorize(Roles = "SuperAdmin,HotelManager,Accountant")]
        [ProducesResponseType(typeof(ApiResponse<AuditResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuditResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuditResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Run([FromBody] AuditRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "System";

            var result = await _nightAuditService.RunNightAuditAsync(userId, request?.ForceClose ?? false);

            if (!result.Succeeded)
            {
                // ApiResponse here uses Succeeded/Message; treat non-success as 400 to bubble the message.
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }

            return Ok(result);
        }
    }
}

