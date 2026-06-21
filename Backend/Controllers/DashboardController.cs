using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves aggregated KPIs and trends for the Executive Dashboard.
    /// </summary>
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// Gets top-level marketing metrics.
        /// </summary>
        [HttpGet("kpis")]
        public async Task<IActionResult> GetDashboardKpis()
        {
            var kpis = await _dashboardService.GetDashboardKpisAsync();
            return Ok(ApiResponse<DashboardKpisDto>.Ok(kpis));
        }

        /// <summary>
        /// Gets monthly revenue values grouped by start date.
        /// </summary>
        [HttpGet("revenue-trend")]
        public async Task<IActionResult> GetRevenueTrend()
        {
            var trend = await _dashboardService.GetRevenueTrendAsync();
            return Ok(ApiResponse<IEnumerable<RevenueTrendDto>>.Ok(trend));
        }

        /// <summary>
        /// Gets top 10 marketing campaigns ranked by ROI.
        /// </summary>
        [HttpGet("top-campaigns")]
        public async Task<IActionResult> GetTopCampaigns()
        {
            var campaigns = await _dashboardService.GetTopCampaignsAsync();
            return Ok(ApiResponse<IEnumerable<CampaignDto>>.Ok(campaigns));
        }
    }
}
