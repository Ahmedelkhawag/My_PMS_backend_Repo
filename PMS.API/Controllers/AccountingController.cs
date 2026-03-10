using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
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

        [HttpPost("accounts")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<int>("Invalid account payload."));
            }

            var result = await _accountingService.CreateAccountAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("accounts/tree")]
        [ProducesResponseType(typeof(ApiResponse<List<AccountTreeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountsTree()
        {
            var result = await _accountingService.GetAccountsTreeAsync();

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("cost-centers")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<int>("Invalid cost center payload."));
            }

            var result = await _accountingService.CreateCostCenterAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("cost-centers/tree")]
        [ProducesResponseType(typeof(ApiResponse<List<CostCenterDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCostCentersTree()
        {
            var result = await _accountingService.GetCostCentersTreeAsync();

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("reports/pnl")]
        [ProducesResponseType(typeof(ApiResponse<PnLReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPnLReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? costCenterId)
        {
            var result = await _accountingService.GetPnLReportAsync(startDate, endDate, costCenterId);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("reports/balance-sheet")]
        [ProducesResponseType(typeof(ApiResponse<BalanceSheetDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
        {
            var result = await _accountingService.GetBalanceSheetAsync(asOfDate);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
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

        [HttpPatch("journal-entries/{id}/approve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveJournalEntry(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>("User not authenticated."));
            }

            var result = await _accountingService.ApproveJournalEntryAsync(id, userId);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPatch("journal-entries/{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RejectJournalEntry(int id, [FromBody] RejectJournalEntryDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>("User not authenticated."));
            }

            if (string.IsNullOrWhiteSpace(dto?.Reason))
            {
                return BadRequest(new ApiResponse<bool>("Rejection reason must be provided."));
            }

            var result = await _accountingService.RejectJournalEntryAsync(id, userId, dto.Reason);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

