using System.Threading.Tasks;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for exporting SQL Server data as clean, Tableau-ready CSV datasets.
    /// </summary>
    public interface ITableauExportService
    {
        /// <summary>
        /// Exports campaign performance data to Tableau/Datasets/campaign_performance.csv.
        /// </summary>
        Task<string> ExportCampaignPerformanceAsync();

        /// <summary>
        /// Exports customer analytics data to Tableau/Datasets/customer_analytics.csv.
        /// </summary>
        Task<string> ExportCustomerAnalyticsAsync();

        /// <summary>
        /// Exports overall campaign summary KPIs to Tableau/Datasets/campaign_summary.csv.
        /// </summary>
        Task<string> ExportCampaignSummaryAsync();

        /// <summary>
        /// Exports monthly revenue and spends trend to Tableau/Datasets/monthly_revenue.csv.
        /// </summary>
        Task<string> ExportMonthlyRevenueAsync();

        /// <summary>
        /// Triggers all CSV exports together.
        /// </summary>
        Task<string> ExportAllAsync();
    }
}
