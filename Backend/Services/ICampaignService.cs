using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    public interface ICampaignService
    {
        Task<IEnumerable<CampaignPerformanceDto>> GetCampaignPerformanceAsync();
        Task<IEnumerable<CampaignComparisonDto>> GetCampaignComparisonAsync();
        Task<IEnumerable<CampaignEffectivenessDto>> GetCampaignEffectivenessAsync();
    }
}
