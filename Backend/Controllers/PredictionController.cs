using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves customer campaign response likelihood prediction APIs backed by Logistic Regression.
    /// </summary>
    [ApiController]
    [Route("api/prediction")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionController(IPredictionService predictionService)
        {
            _predictionService = predictionService ?? throw new ArgumentNullException(nameof(predictionService));
        }

        /// <summary>
        /// Manually triggers model training and refreshes the metrics report.
        /// </summary>
        [HttpPost("train")]
        public async Task<IActionResult> TrainModel()
        {
            var success = await _predictionService.TrainModelAsync();
            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail("Model training pipeline execution failed. Check logs."));
            }
            return Ok(ApiResponse<string>.Ok("Logistic Regression model trained and metrics saved successfully."));
        }

        /// <summary>
        /// Predicts likely campaign response.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PredictResponse([FromBody] PredictionRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<PredictionResponseDto>.Fail("Prediction request payload cannot be empty."));
            }

            var response = await _predictionService.PredictAsync(request);
            return Ok(ApiResponse<PredictionResponseDto>.Ok(response));
        }

        /// <summary>
        /// Retrieves the pre-calculated model metrics.
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetModelMetrics()
        {
            var metrics = await _predictionService.GetMetricsAsync();
            if (metrics == null)
            {
                return NotFound(ApiResponse<PredictionMetricsDto>.Fail("Model metrics file not found. Ensure model is trained first."));
            }
            return Ok(ApiResponse<PredictionMetricsDto>.Ok(metrics));
        }
    }
}
