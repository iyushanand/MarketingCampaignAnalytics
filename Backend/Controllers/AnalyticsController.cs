using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        [HttpGet("eda")]
        public async Task<IActionResult> GetEdaResults()
        {
            var result = await _analyticsService.GetEdaResultsAsync();
            return Ok(result);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatisticsResults()
        {
            var result = await _analyticsService.GetStatisticsResultsAsync();
            return Ok(result);
        }

        [HttpPost("predict")]
        public async Task<IActionResult> PredictResponse([FromBody] PredictionRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Invalid prediction parameters." });
            }

            var result = await _analyticsService.PredictResponseAsync(request);
            return Ok(result);
        }
    }
}
