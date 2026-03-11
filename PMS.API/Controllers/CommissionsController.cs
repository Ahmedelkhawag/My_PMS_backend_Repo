using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/commissions")]
    [ApiController]
    [Authorize(Roles = "Accountant,HotelManager,SuperAdmin")]
    public class CommissionsController : ControllerBase
    {
        private readonly ICommissionService _commissionService;

        public CommissionsController(ICommissionService commissionService)
        {
            _commissionService = commissionService;
        }

        /// <summary>
        /// Returns all Draft (pending) TA commission records.
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TACommissionRecordDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingCommissions()
        {
            var result = await _commissionService.GetPendingCommissionsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Manually triggers commission calculation for a specific reservation.
        /// Useful when a folio was modified after the automatic calculation ran.
        /// </summary>
        [HttpPost("calculate/{reservationId:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Calculate(int reservationId)
        {
            var result = await _commissionService.CalculateForReservationAsync(reservationId);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Approves a Draft commission record, posts the GL Journal Entry,
        /// and marks the record as Approved.
        /// </summary>
        [HttpPost("{id:int}/approve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _commissionService.ApproveCommissionAsync(id);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
