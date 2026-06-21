using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for Machine Learning Response Prediction operations.
    /// </summary>
    public interface IPredictionService
    {
        /// <summary>
        /// Trains the Logistic Regression model using clean database records.
        /// </summary>
        Task<bool> TrainModelAsync();

        /// <summary>
        /// Predicts campaign response likelihood for a specific customer profile.
        /// </summary>
        Task<PredictionResponseDto> PredictAsync(PredictionRequestDto request);

        /// <summary>
        /// Gets the Logistic Regression model's evaluation metrics.
        /// </summary>
        Task<PredictionMetricsDto?> GetMetricsAsync();
    }
}
