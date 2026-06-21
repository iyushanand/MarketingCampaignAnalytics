using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves predefined marketing reports in JSON format.
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
        /// Gets predefined campaign summary metrics.
        /// </summary>
        [HttpGet("campaign")]
        public async Task<IActionResult> GetCampaignReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("campaign");
            return Content(dataJson, "application/json");
        }

        /// <summary>
        /// Gets customer lifetime value and purchase counts.
        /// </summary>
        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomerReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("customer");
            return Content(dataJson, "application/json");
        }

        /// <summary>
        /// Gets marketing channel effectiveness (spend, conversions, revenue, ROI).
        /// </summary>
        [HttpGet("channel")]
        public async Task<IActionResult> GetChannelReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("channel");
            return Content(dataJson, "application/json");
        }

        /// <summary>
        /// Gets monthly breakdown of revenue and marketing spends.
        /// </summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport()
        {
            var dataJson = await _reportService.GetMarketingReportDataAsync("monthly");
            return Content(dataJson, "application/json");
        }
    }
}
