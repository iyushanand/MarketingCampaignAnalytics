using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    public interface IAnalyticsService
    {
        Task<EdaResultDto> GetEdaResultsAsync();
        Task<StatisticsResultDto> GetStatisticsResultsAsync();
        Task<PredictionResponseDto> PredictResponseAsync(PredictionRequestDto request);
    }
}
