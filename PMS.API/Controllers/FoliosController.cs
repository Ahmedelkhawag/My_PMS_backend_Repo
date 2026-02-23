using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Folios;
using PMS.Application.Interfaces.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/folios")]
    [ApiController]
    [Authorize]
    public class FoliosController : ControllerBase
    {
        private readonly IFolioService _folioService;

        public FoliosController(IFolioService folioService)
        {
            _folioService = folioService;
        }

        /// <summary>
        /// Adds a new transaction to the folio of the given reservation.
        /// </summary>
        [HttpPost("transaction")]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddTransaction([FromBody] CreateTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات المعاملة غير صحيحة", 400));

            var result = await _folioService.AddTransactionAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("transactions/{id}/void")]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VoidTransaction(int id)
        {
            var result = await _folioService.VoidTransactionAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpGet("{reservationId}")]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFolioByReservation(int reservationId)
        {
            var result = await _folioService.GetFolioDetailsAsync(reservationId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 404, result);

            return Ok(result);
        }

        [HttpGet("{reservationId}/summary")]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestFolioSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFolioSummary(int reservationId)
        {
            var result = await _folioService.GetFolioSummaryAsync(reservationId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPost("post-payment")]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostPaymentWithDiscount([FromBody] PostPaymentWithDiscountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات الدفع والخصم غير مكتملة", 400));

            var result = await _folioService.PostPaymentWithDiscountAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPost("transactions/{id}/refund")]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefundTransaction(int id, [FromBody] RefundTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات الاسترجاع غير صحيحة", 400));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ResponseObjectDto<string>.Failure("لم يتم التعرف على المستخدم", 401));

            var result = await _folioService.RefundTransactionAsync(id, dto.Amount, dto.Reason, userId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        [HttpPost("transactions/{id}/transfer")]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TransferTransaction(int id, [FromBody] TransferTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات التحويل غير صحيحة", 400));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ResponseObjectDto<string>.Failure("لم يتم التعرف على المستخدم", 401));

            var result = await _folioService.TransferTransactionAsync(id, dto.TargetReservationId, userId, dto.Reason);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }
    }
}

