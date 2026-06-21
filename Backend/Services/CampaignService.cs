using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.DTOs;
using Backend.Models;
using Backend.Repository;

namespace Backend.Services
{
    /// <summary>
    /// Implements campaign analytics and comparisons.
    /// </summary>
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly ICampaignResponseRepository _responseRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignService"/> class.
        /// </summary>
        public CampaignService(
            ICampaignRepository campaignRepository,
            ICampaignResponseRepository responseRepository)
        {
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
        }

        /// <summary>
        /// Gets all campaigns.
        /// </summary>
        public async Task<IEnumerable<CampaignDto>> GetCampaignsAsync()
        {
            var campaigns = await _campaignRepository.GetAllAsync();
            return campaigns.Select(c => MapToDto(c));
        }

        /// <summary>
        /// Gets campaign by ID.
        /// </summary>
        public async Task<CampaignDto?> GetCampaignByIdAsync(int id)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id);
            return campaign == null ? null : MapToDto(campaign);
        }

        /// <summary>
        /// Calculates detailed performance for all campaigns.
        /// </summary>
        public async Task<IEnumerable<CampaignPerformanceDto>> GetCampaignPerformanceAsync()
        {
            var campaigns = await _campaignRepository.GetAllAsync();
            return campaigns.Select(c => new CampaignPerformanceDto
            {
                CampaignName = c.CampaignName,
                Spend = c.Spend,
                Revenue = c.Revenue,
                Roi = c.Spend > 0 ? Math.Round((c.Revenue - c.Spend) / c.Spend, 4) : 0,
                ConversionRate = c.Impressions > 0 ? Math.Round((decimal)c.Conversions / c.Impressions, 4) : 0,
                Ctr = c.Impressions > 0 ? Math.Round((double)c.Clicks / c.Impressions, 4) : 0
            });
        }

        /// <summary>
        /// Calculates campaign comparison parameters.
        /// </summary>
        public async Task<IEnumerable<CampaignComparisonDto>> GetCampaignComparisonAsync()
        {
            var campaigns = await _campaignRepository.GetAllAsync();
            return campaigns.Select(c => new CampaignComparisonDto
            {
                CampaignName = c.CampaignName,
                Spend = c.Spend,
                Revenue = c.Revenue,
                Roi = c.Spend > 0 ? Math.Round((c.Revenue - c.Spend) / c.Spend, 4) : 0,
                ConversionRate = c.Impressions > 0 ? Math.Round((decimal)c.Conversions / c.Impressions, 4) : 0
            });
        }

        /// <summary>
        /// Calculates campaign effectiveness (ROI, Response Rates, and best/worst markers).
        /// </summary>
        public async Task<IEnumerable<CampaignEffectivenessDto>> GetCampaignEffectivenessAsync()
        {
            var campaigns = (await _campaignRepository.GetAllAsync()).ToList();
            if (!campaigns.Any())
            {
                return Enumerable.Empty<CampaignEffectivenessDto>();
            }

            // Group responses to get yes/total counts
            var responseStats = await _responseRepository.Query()
                .GroupBy(r => r.CampaignId)
                .Select(g => new {
                    CampaignId = g.Key,
                    YesCount = g.Count(r => r.Response == "Yes"),
                    TotalCount = g.Count()
                })
                .ToDictionaryAsync(x => x.CampaignId, x => x);

            var effectivenessList = new List<CampaignEffectivenessDto>();

            foreach (var c in campaigns)
            {
                decimal responseRate = 0;
                if (responseStats.TryGetValue(c.CampaignId, out var stats) && stats.TotalCount > 0)
                {
                    responseRate = (decimal)stats.YesCount / stats.TotalCount;
                }

                effectivenessList.Add(new CampaignEffectivenessDto
                {
                    CampaignName = c.CampaignName,
                    Spend = c.Spend,
                    Revenue = c.Revenue,
                    Roi = c.Spend > 0 ? Math.Round((c.Revenue - c.Spend) / c.Spend, 4) : 0,
                    ConversionRate = c.Impressions > 0 ? Math.Round((decimal)c.Conversions / c.Impressions, 4) : 0,
                    ResponseRate = Math.Round(responseRate, 4),
                    IsBestRoi = false,
                    IsWorstRoi = false,
                    IsBestResponse = false
                });
            }

            // Determine best/worst markers
            var bestRoiCampaign = effectivenessList.OrderByDescending(x => x.Roi).FirstOrDefault();
            var worstRoiCampaign = effectivenessList.OrderBy(x => x.Roi).FirstOrDefault();
            var bestResponseCampaign = effectivenessList.OrderByDescending(x => x.ResponseRate).FirstOrDefault();

            if (bestRoiCampaign != null) bestRoiCampaign.IsBestRoi = true;
            if (worstRoiCampaign != null) worstRoiCampaign.IsWorstRoi = true;
            if (bestResponseCampaign != null) bestResponseCampaign.IsBestResponse = true;

            return effectivenessList;
        }

        private static CampaignDto MapToDto(Campaign c)
        {
            return new CampaignDto
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
            };
        }
    }
}
