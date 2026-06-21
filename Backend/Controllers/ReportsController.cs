using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves marketing reports, automated document exports, and download history.
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsController"/> class.
        /// </summary>
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

        /// <summary>
        /// Generates and triggers download for the professional Excel performance report.
        /// </summary>
        [HttpGet("excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadExcelReport()
        {
            try
            {
                string filePath = await _reportService.GenerateExcelReportAsync();
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string fileName = Path.GetFileName(filePath);
                return base.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to generate Excel report. Details: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generates and triggers download for the professional PDF marketing report.
        /// </summary>
        [HttpGet("pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadPdfReport()
        {
            try
            {
                string filePath = await _reportService.GeneratePdfReportAsync();
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string fileName = Path.GetFileName(filePath);
                return base.File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to generate PDF report. Details: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves a metadata list of all previously generated Excel and PDF reports.
        /// </summary>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportList()
        {
            var list = await _reportService.GetGeneratedReportsAsync();
            return Ok(ApiResponse<System.Collections.Generic.IEnumerable<ReportFileDto>>.Ok(list));
        }

        /// <summary>
        /// Serves an archived report file for download.
        /// </summary>
        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadArchivedReport([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(ApiResponse<string>.Fail("File name parameter is required."));
            }

            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            string subFolder = ext == ".xlsx" ? "Excel" : ext == ".pdf" ? "PDF" : string.Empty;

            if (string.IsNullOrEmpty(subFolder))
            {
                return BadRequest(ApiResponse<string>.Fail("Invalid file type requested."));
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", subFolder, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(ApiResponse<string>.Fail($"Requested report '{fileName}' does not exist on disk."));
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            string contentType = ext == ".xlsx" 
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" 
                : "application/pdf";

            return base.File(bytes, contentType, fileName);
        }

        /// <summary>
        /// Gets predefined campaign summary metrics (legacy grid support).
        /// </summary>
        [HttpGet("campaign")]
        public async Task<IActionResult> GetCampaignReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("campaign");
            return Content(dataJson, "application/json");
        }

        /// <summary>
        /// Gets customer lifetime value and purchase counts (legacy grid support).
        /// </summary>
        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomerReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("customer");
            return Content(dataJson, "application/json");
        }

        /// <summary>
        /// Gets marketing channel effectiveness (spend, conversions, revenue, ROI) (legacy grid support).
        /// </summary>
        [HttpGet("channel")]
        public async Task<IActionResult> GetChannelReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("channel");
            return Content(dataJson, "application/json");
        }

        /// <summary>
        /// Gets monthly breakdown of revenue and marketing spends (legacy grid support).
        /// </summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("monthly");
            return Content(dataJson, "application/json");
        }
    }
}
