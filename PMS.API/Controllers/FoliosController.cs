using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Folios;
using PMS.Application.Interfaces.Services;
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
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddTransaction([FromBody] CreateTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _folioService.AddTransactionAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Voids an existing folio transaction by creating a reversal entry.
        /// </summary>
        [HttpPost("transactions/{id}/void")]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioTransactionDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoidTransaction(int id)
        {
            var result = await _folioService.VoidTransactionAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        /// <summary>
        /// Returns the full folio details and transaction ledger for the given reservation.
        /// </summary>
        [HttpGet("{reservationId}")]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioDetailsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioDetailsDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<FolioDetailsDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFolioByReservation(int reservationId)
        {
            var result = await _folioService.GetFolioDetailsAsync(reservationId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }

        /// <summary>
        /// Returns a lightweight summary of the folio for the given reservation.
        /// </summary>
        [HttpGet("{reservationId}/summary")]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestFolioSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestFolioSummaryDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestFolioSummaryDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<GuestFolioSummaryDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFolioSummary(int reservationId)
        {
            var result = await _folioService.GetFolioSummaryAsync(reservationId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return Ok(result);
        }
    }
}

