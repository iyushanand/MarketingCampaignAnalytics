using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/campaign")]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetCampaignPerformance()
        {
            var performance = await _campaignService.GetCampaignPerformanceAsync();
            return Ok(performance);
        }

        [HttpGet("comparison")]
        public async Task<IActionResult> GetCampaignComparison()
        {
            var comparison = await _campaignService.GetCampaignComparisonAsync();
            return Ok(comparison);
        }

        [HttpGet("effectiveness")]
        public async Task<IActionResult> GetCampaignEffectiveness()
        {
            var effectiveness = await _campaignService.GetCampaignEffectivenessAsync();
            return Ok(effectiveness);
        }
    }
}
