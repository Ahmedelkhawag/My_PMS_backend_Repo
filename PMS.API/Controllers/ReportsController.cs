using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }


        /// <summary>
        /// Downloads the daily police report as an Excel file.
        /// </summary>
        /// <param name="date">Optional: The business date for the report. Defaults to current open business date.</param>
        /// <returns>Excel File (.xlsx)</returns>
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
    }
}
