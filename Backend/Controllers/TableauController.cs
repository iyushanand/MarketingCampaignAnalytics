using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.DTOs;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves Tableau BI exports and dataset downloads.
    /// </summary>
    [ApiController]
    [Route("api/tableau")]
    public class TableauController : ControllerBase
    {
        private readonly ITableauExportService _exportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableauController"/> class.
        /// </summary>
        public TableauController(ITableauExportService exportService)
        {
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        }

        /// <summary>
        /// Exports and downloads campaign performance CSV.
        /// </summary>
        [HttpGet("export/campaign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportCampaignPerformance()
        {
            try
            {
                string filePath = await _exportService.ExportCampaignPerformanceAsync();
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return base.File(bytes, "text/csv", "campaign_performance.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to export campaign performance. Details: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exports and downloads customer analytics CSV.
        /// </summary>
        [HttpGet("export/customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportCustomerAnalytics()
        {
            try
            {
                string filePath = await _exportService.ExportCustomerAnalyticsAsync();
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return base.File(bytes, "text/csv", "customer_analytics.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to export customer analytics. Details: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exports and downloads overall campaign summary CSV.
        /// </summary>
        [HttpGet("export/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportCampaignSummary()
        {
            try
            {
                string filePath = await _exportService.ExportCampaignSummaryAsync();
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return base.File(bytes, "text/csv", "campaign_summary.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to export campaign summary. Details: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exports and downloads monthly revenue trend CSV.
        /// </summary>
        [HttpGet("export/monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportMonthlyRevenue()
        {
            try
            {
                string filePath = await _exportService.ExportMonthlyRevenueAsync();
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return base.File(bytes, "text/csv", "monthly_revenue.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to export monthly revenue. Details: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exports all Tableau datasets together and returns target path info.
        /// </summary>
        [HttpGet("export/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportAll()
        {
            try
            {
                string path = await _exportService.ExportAllAsync();
                return Ok(ApiResponse<string>.Ok(path, "All datasets exported successfully to Tableau/Datasets/ directory."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Failed to export all datasets. Details: {ex.Message}"));
            }
        }
    }
}
