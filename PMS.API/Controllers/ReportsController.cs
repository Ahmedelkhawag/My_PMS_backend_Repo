using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces.Services;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Shifts;
using System.Security.Claims;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IAuthService _authService;
        private readonly IShiftService _shiftService;
        private readonly IRegistrationCardPdfService _registrationCardPdfService;

        public ReportsController(
            IReportService reportService,
            IAuthService authService,
            IShiftService shiftService,
            IRegistrationCardPdfService registrationCardPdfService)
        {
            _reportService = reportService;
            _authService = authService;
            _shiftService = shiftService;
            _registrationCardPdfService = registrationCardPdfService;
        }


        /// <summary>
        /// Downloads the daily police report as an Excel file.
        /// </summary>
        /// <param name="date">Optional: The business date for the report. Defaults to current open business date.</param>
        /// <returns>Excel File (.xlsx)</returns>
        /// 
        [Authorize(Roles = "HotelManager,SuperAdmin,Receptionist")]
        [HttpGet("police-report")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadPoliceReport([FromQuery] DateTime? date)
        {
            try
            {
                // 1. Generate the Excel file bytes
                var fileContent = await _reportService.GeneratePoliceReportAsync(date);

                // 2. Define the file name (e.g., Police_Report_18_02_2026.xlsx)
                var reportDate = date ?? DateTime.UtcNow.Date; // Just for naming fallback
                var fileName = $"Police_Report_{reportDate:dd_MM_yyyy}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // 3. Return the file
                return File(fileContent, contentType, fileName);
            }
            catch (Exception ex)
            {
                // In case of error, return a Bad Request with the message
                return BadRequest(new { message = "Failed to generate report", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على تقرير الوردية الحالية للمستخدم.
        /// </summary>
        /// 
        [Authorize]
        [HttpGet("shift/current")]
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
        /// Generates the bilingual (Arabic/English) Guest Registration Card PDF for the reservation.
        /// </summary>
        /// 
        [Authorize(Roles = "HotelManager,SuperAdmin,Receptionist")]
        [HttpGet("registration-card/{id}")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistrationCard(int id)
        {
            string? receptionistName = "—";
            var profileResult = await _authService.GetCurrentUserProfileAsync();
            if (profileResult.Succeeded && profileResult.Data != null)
                receptionistName = profileResult.Data.FullName;

            var result = await _registrationCardPdfService.GenerateRegistrationCardAsync(id, receptionistName);

            if (result == null)
                return NotFound(ResponseObjectDto<string>.Failure("Reservation not found.", 404));

            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }
    }
}
