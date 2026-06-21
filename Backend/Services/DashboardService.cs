using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.DTOs;
using Backend.Repository;

namespace Backend.Services
{
    /// <summary>
    /// Implements Executive Dashboard query aggregations using EF Core and LINQ.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICampaignResponseRepository _responseRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        public DashboardService(
            ICampaignRepository campaignRepository,
            ICustomerRepository customerRepository,
            ICampaignResponseRepository responseRepository)
        {
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
        }

        /// <summary>
        /// Gets the executive dashboard KPIs.
        /// </summary>
        public async Task<DashboardKpisDto> GetDashboardKpisAsync()
        {
            var totalRevenue = await _campaignRepository.Query().SumAsync(c => c.Revenue);
            var campaignSpend = await _campaignRepository.Query().SumAsync(c => c.Spend);
            
            decimal roi = 0;
            if (campaignSpend > 0)
            {
                roi = (totalRevenue - campaignSpend) / campaignSpend;
            }

            var totalCampaigns = await _campaignRepository.Query().CountAsync();
            var totalCustomers = await _customerRepository.Query().CountAsync();

            // Average Order Value (AOV): Average purchase amount of positive responses
            var responseQuery = _responseRepository.Query()
                .Where(r => r.Response == "Yes" && r.PurchaseAmount > 0);

            decimal averageOrderValue = 0;
            if (await responseQuery.AnyAsync())
            {
                averageOrderValue = await responseQuery.AverageAsync(r => r.PurchaseAmount);
            }

            // Best Marketing Channel by Revenue
            var bestChannelGroup = await _campaignRepository.Query()
                .GroupBy(c => c.MarketingChannel)
                .Select(g => new { Channel = g.Key, TotalRevenue = g.Sum(c => c.Revenue) })
                .OrderByDescending(x => x.TotalRevenue)
                .FirstOrDefaultAsync();

            string bestChannel = bestChannelGroup?.Channel ?? "N/A";

            // Conversion Rate: Conversions / Impressions
            var totalImpressions = await _campaignRepository.Query().SumAsync(c => c.Impressions);
            var totalConversions = await _campaignRepository.Query().SumAsync(c => c.Conversions);
            decimal conversionRate = 0;
            if (totalImpressions > 0)
            {
                conversionRate = (decimal)totalConversions / totalImpressions;
            }

            return new DashboardKpisDto
            {
                TotalRevenue = totalRevenue,
                CampaignSpend = campaignSpend,
                Roi = roi,
                TotalCampaigns = totalCampaigns,
                TotalCustomers = totalCustomers,
                AverageOrderValue = Math.Round(averageOrderValue, 2),
                BestMarketingChannel = bestChannel,
                ConversionRate = Math.Round(conversionRate, 4)
            };
        }

        /// <summary>
        /// Gets the monthly revenue trend.
        /// </summary>
        public async Task<IEnumerable<RevenueTrendDto>> GetRevenueTrendAsync()
        {
            // Query campaigns, group by start date month
            var campaigns = await _campaignRepository.GetAllAsync();
            
            var trend = campaigns
                .GroupBy(c => c.StartDate.ToString("yyyy-MM"))
                .Select(g => new RevenueTrendDto
                {
                    Month = g.Key,
                    Revenue = g.Sum(c => c.Revenue)
                })
                .OrderBy(x => x.Month)
                .ToList();

            return trend;
        }

        /// <summary>
        /// Gets the top 10 campaigns by ROI.
        /// </summary>
        public async Task<IEnumerable<CampaignDto>> GetTopCampaignsAsync()
        {
            var campaigns = await _campaignRepository.Query()
                .Select(c => new {
                    Campaign = c,
                    Roi = c.Spend > 0 ? (c.Revenue - c.Spend) / c.Spend : 0
                })
                .OrderByDescending(x => x.Roi)
                .Take(10)
                .Select(x => x.Campaign)
                .ToListAsync();

            return campaigns.Select(c => new CampaignDto
            {
                CampaignId = c.CampaignId,
                CampaignName = c.CampaignName,
                CampaignType = c.CampaignType,
                MarketingChannel = c.MarketingChannel,
                Budget = c.Budget,
                Spend = c.Spend,
                Revenue = c.Revenue,
                Conversions = c.Conversions,
                Clicks = c.Clicks,
                Impressions = c.Impressions,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            });
        }
    }
}
