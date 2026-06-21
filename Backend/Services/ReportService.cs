using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Database;

namespace Backend.Services
{
    /// <summary>
    /// Implements reporting query logic returning formatted JSON reports.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly PythonRunner _pythonRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        public ReportService(ApplicationDbContext context, PythonRunner pythonRunner)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pythonRunner = pythonRunner ?? throw new ArgumentNullException(nameof(pythonRunner));
        }

        /// <summary>
        /// Gets predefined marketing report data in JSON format.
        /// </summary>
        public async Task<string> GetMarketingReportDataAsync(string reportType)
        {
            object? reportData = null;

            switch (reportType.ToLowerInvariant())
            {
                case "campaign":
                    reportData = await _context.Campaigns
                        .Select(c => new
                        {
                            c.CampaignId,
                            c.CampaignName,
                            c.CampaignType,
                            c.MarketingChannel,
                            c.Budget,
                            c.Spend,
                            c.Revenue,
                            c.Conversions,
                            Roi = c.Spend > 0 ? Math.Round((c.Revenue - c.Spend) / c.Spend, 4) : 0
                        })
                        .ToListAsync();
                    break;

                case "customer":
                    reportData = await _context.Customers
                        .Select(c => new
                        {
                            c.CustomerId,
                            c.FirstName,
                            c.LastName,
                            c.Age,
                            c.Gender,
                            c.Income,
                            c.Country,
                            TotalSpend = _context.CampaignResponses
                                .Where(r => r.CustomerId == c.CustomerId)
                                .Sum(r => r.PurchaseAmount),
                            TotalPurchases = _context.CampaignResponses
                                .Where(r => r.CustomerId == c.CustomerId)
                                .Sum(r => r.NumberOfPurchases)
                        })
                        .Take(500) // Cap to avoid huge response payloads
                        .ToListAsync();
                    break;

                case "channel":
                    reportData = await _context.Campaigns
                        .GroupBy(c => c.MarketingChannel)
                        .Select(g => new
                        {
                            Channel = g.Key,
                            TotalCampaigns = g.Count(),
                            TotalSpend = g.Sum(c => c.Spend),
                            TotalRevenue = g.Sum(c => c.Revenue),
                            TotalConversions = g.Sum(c => c.Conversions),
                            Roi = g.Sum(c => c.Spend) > 0 
                                ? Math.Round((g.Sum(c => c.Revenue) - g.Sum(c => c.Spend)) / g.Sum(c => c.Spend), 4) 
                                : 0
                        })
                        .ToListAsync();
                    break;

                case "monthly":
                    var campaigns = await _context.Campaigns.ToListAsync();
                    reportData = campaigns
                        .GroupBy(c => c.StartDate.ToString("yyyy-MM"))
                        .Select(g => new
                        {
                            Month = g.Key,
                            Spend = g.Sum(c => c.Spend),
                            Revenue = g.Sum(c => c.Revenue),
                            Conversions = g.Sum(c => c.Conversions),
                            Roi = g.Sum(c => c.Spend) > 0 
                                ? Math.Round((g.Sum(c => c.Revenue) - g.Sum(c => c.Spend)) / g.Sum(c => c.Spend), 4) 
                                : 0
                        })
                        .OrderBy(x => x.Month)
                        .ToList();
                    break;

                default:
                    throw new ArgumentException($"Invalid report type: '{reportType}'", nameof(reportType));
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
            return JsonSerializer.Serialize(reportData, options);
        }

        /// <summary>
        /// Placeholder for Excel file generation (Implemented in Phase 9).
        /// </summary>
        public async Task<byte[]> GenerateExcelReportAsync()
        {
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        /// <summary>
        /// Placeholder for PDF file generation (Implemented in Phase 9).
        /// </summary>
        public async Task<byte[]> GeneratePdfReportAsync()
        {
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }
    }
}
