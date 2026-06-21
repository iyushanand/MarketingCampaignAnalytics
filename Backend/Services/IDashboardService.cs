using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for Executive Dashboard analytics.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets the executive dashboard KPIs.
        /// </summary>
        Task<DashboardKpisDto> GetDashboardKpisAsync();

        /// <summary>
        /// Gets the monthly revenue trend.
        /// </summary>
        Task<IEnumerable<RevenueTrendDto>> GetRevenueTrendAsync();

        /// <summary>
        /// Gets the top 10 campaigns by ROI.
        /// </summary>
        Task<IEnumerable<CampaignDto>> GetTopCampaignsAsync();
    }
}
