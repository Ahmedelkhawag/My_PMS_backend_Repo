using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/accounting")]
    [ApiController]
    [Authorize(Roles = "Accountant,HotelManager,SuperAdmin")]
    public class AccountingController : ControllerBase
    {
        private readonly IAccountingService _accountingService;

        public AccountingController(IAccountingService accountingService)
        {
            _accountingService = accountingService;
        }

        [HttpPost("journal-entries")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateManualJournalEntry([FromBody] CreateJournalEntryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>("Invalid journal entry payload."));
            }

            var result = await _accountingService.CreateManualJournalEntryAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("trial-balance")]
        [ProducesResponseType(typeof(ApiResponse<TrialBalanceReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrialBalance()
        {
            var result = await _accountingService.GetTrialBalanceAsync();

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("accounts/{id}/statement")]
        [ProducesResponseType(typeof(ApiResponse<AccountStatementHeaderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountStatementHeaderDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAccountStatement(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new ApiResponse<AccountStatementHeaderDto>("Start date must be before or equal to end date."));
            }

            var result = await _accountingService.GetAccountStatementAsync(id, startDate, endDate);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

