using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/ar")]
    [ApiController]
    [Authorize(Roles = "Accountant,HotelManager,SuperAdmin")]
    public class ARController : ControllerBase
    {
        private readonly IARService _arService;

        public ARController(IARService arService)
        {
            _arService = arService;
        }

        [HttpPost("transfer-folio")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TransferFolioToAR([FromBody] TransferFolioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>("Invalid transfer payload."));
            }

            var result = await _arService.TransferFolioToARAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("payments")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReceiveARPayment([FromBody] ReceiveARPaymentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>("Invalid payment payload."));
            }

            var result = await _arService.ReceiveARPaymentAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("companies/{companyId:int}/statement")]
        [ProducesResponseType(typeof(ApiResponse<CompanyStatementReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CompanyStatementReportDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCompanyStatement(
            int companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new ApiResponse<CompanyStatementReportDto>("Start date must be before or equal to end date."));
            }

            var result = await _arService.GetCompanyStatementAsync(companyId, startDate, endDate);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("companies/{companyId:int}/statement/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCompanyStatementPdf(
            int companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new ApiResponse<byte[]>("Start date must be before or equal to end date."));
            }

            var result = await _arService.GenerateCompanySOAInPdfAsync(companyId, startDate, endDate);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            var companyName = SanitizeFileName(result.Data!.CompanyName);
            var fileName = $"SOA_{companyName}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";

            return File(result.Data.PdfBytes, "application/pdf", fileName);
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Company";
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Concat(name.Where(c => !invalid.Contains(c))).Trim();
            return string.IsNullOrEmpty(sanitized) ? "Company" : sanitized;
        }

        [HttpGet("reports/aging")]
        [ProducesResponseType(typeof(ApiResponse<ARAgingReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetARAgingReport()
        {
            var result = await _arService.GetARAgingReportAsync();
            return Ok(result);
        }

        [HttpPost("adjustments")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAdjustment([FromBody] CreateARAdjustmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>("Invalid adjustment payload."));
            }

            var result = await _arService.CreateAdjustmentAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // ── Stage 2: Dispute Management ──────────────────────────────────────

        [HttpPost("invoices/{id:int}/dispute")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisputeInvoice(int id, [FromBody] DisputeInvoiceDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new ApiResponse<bool>("Invalid dispute payload. A reason is required."));
            }

            var result = await _arService.MarkInvoiceAsDisputedAsync(id, dto.Reason);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("invoices/{id:int}/resolve-dispute")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResolveDispute(int id)
        {
            var result = await _arService.ResolveInvoiceDisputeAsync(id);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    public record DisputeInvoiceDto(string Reason);
}

