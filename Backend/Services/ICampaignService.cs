using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for campaign analytics and performance.
    /// </summary>
    public interface ICampaignService
    {
        /// <summary>
        /// Gets all campaigns.
        /// </summary>
        Task<IEnumerable<CampaignDto>> GetCampaignsAsync();

        /// <summary>
        /// Gets a campaign by its ID.
        /// </summary>
        Task<CampaignDto?> GetCampaignByIdAsync(int id);

        /// <summary>
        /// Gets performance metrics for all campaigns.
        /// </summary>
        Task<IEnumerable<CampaignPerformanceDto>> GetCampaignPerformanceAsync();

        /// <summary>
        /// Gets comparison metrics for all campaigns.
        /// </summary>
        Task<IEnumerable<CampaignComparisonDto>> GetCampaignComparisonAsync();

        /// <summary>
        /// Gets effectiveness metrics for all campaigns (ROI, Response Rates, Best/Worst campaign flags).
        /// </summary>
        Task<IEnumerable<CampaignEffectivenessDto>> GetCampaignEffectivenessAsync();
    }
}
