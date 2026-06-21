using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves marketing campaign list, detail, performance, comparison and effectiveness metrics.
    /// </summary>
    [ApiController]
    [Route("api/campaign")]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignController"/> class.
        /// </summary>
        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        }

        /// <summary>
        /// Gets all campaigns from the database.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCampaigns()
        {
            var campaigns = await _campaignService.GetCampaignsAsync();
            return Ok(ApiResponse<IEnumerable<CampaignDto>>.Ok(campaigns));
        }

        /// <summary>
        /// Gets detailed attributes for a single campaign.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound(ApiResponse<CampaignDto>.Fail($"Campaign with ID {id} not found."));
            }
            return Ok(ApiResponse<CampaignDto>.Ok(campaign));
        }

        /// <summary>
        /// Gets detailed performance metrics (Spend, Revenue, ROI, CTR, conversions).
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetCampaignPerformance()
        {
            var performance = await _campaignService.GetCampaignPerformanceAsync();
            return Ok(ApiResponse<IEnumerable<CampaignPerformanceDto>>.Ok(performance));
        }

        /// <summary>
        /// Gets campaign ROI and conversion comparison stats.
        /// </summary>
        [HttpGet("comparison")]
        public async Task<IActionResult> GetCampaignComparison()
        {
            var comparison = await _campaignService.GetCampaignComparisonAsync();
            return Ok(ApiResponse<IEnumerable<CampaignComparisonDto>>.Ok(comparison));
        }

        /// <summary>
        /// Gets campaign effectiveness metrics (ROI, conversion, response rates, and best/worst flags).
        /// </summary>
        [HttpGet("effectiveness")]
        public async Task<IActionResult> GetCampaignEffectiveness()
        {
            var effectiveness = await _campaignService.GetCampaignEffectivenessAsync();
            return Ok(ApiResponse<IEnumerable<CampaignEffectivenessDto>>.Ok(effectiveness));
        }
    }
}
