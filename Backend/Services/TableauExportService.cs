using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Database;
using Backend.Models;

namespace Backend.Services
{
    /// <summary>
    /// Implements automated exporting of SQL Server database records into Tableau-ready CSV files.
    /// </summary>
    public class TableauExportService : ITableauExportService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableauExportService"/> class.
        /// </summary>
        public TableauExportService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            EnsureDirectoryExists();
        }

        private string GetExportDirectory()
        {
            string current = Directory.GetCurrentDirectory();
            if (Path.GetFileName(current).Equals("Backend", StringComparison.OrdinalIgnoreCase))
            {
                current = Path.GetDirectoryName(current) ?? current;
            }
            return Path.Combine(current, "Tableau", "Datasets");
        }

        private void EnsureDirectoryExists()
        {
            string path = GetExportDirectory();
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Exports campaign performance data to Tableau/Datasets/campaign_performance.csv.
        /// </summary>
        public async Task<string> ExportCampaignPerformanceAsync()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var responseStats = await _context.CampaignResponses
                .GroupBy(r => r.CampaignId)
                .Select(g => new {
                    CampaignId = g.Key,
                    TotalResponses = g.Count(),
                    YesResponses = g.Count(r => r.Response == "Yes")
                })
                .ToDictionaryAsync(x => x.CampaignId, x => new { x.TotalResponses, x.YesResponses });

            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("Campaign Name,Marketing Channel,Spend,Revenue,ROI,ROAS,CTR,Conversions,Response Rate,Campaign Status");

            foreach (var c in campaigns)
            {
                double roi = c.Spend > 0 ? (double)(c.Revenue - c.Spend) / (double)c.Spend : 0.0;
                double roas = c.Spend > 0 ? (double)c.Revenue / (double)c.Spend : 0.0;
                double ctr = c.Impressions > 0 ? (double)c.Clicks / c.Impressions : 0.0;

                double responseRate = 0.0;
                if (responseStats.TryGetValue(c.CampaignId, out var stats) && stats.TotalResponses > 0)
                {
                    responseRate = (double)stats.YesResponses / stats.TotalResponses;
                }

                csv.AppendLine($"{EscapeCsv(c.CampaignName)},{EscapeCsv(c.MarketingChannel)},{c.Spend:F2},{c.Revenue:F2},{roi:F4},{roas:F2},{ctr:F4},{c.Conversions},{responseRate:F4},{EscapeCsv(c.Status)}");
            }

            string filePath = Path.Combine(GetExportDirectory(), "campaign_performance.csv");
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Exports customer analytics data to Tableau/Datasets/customer_analytics.csv.
        /// </summary>
        public async Task<string> ExportCustomerAnalyticsAsync()
        {
            var customers = await _context.Customers.ToListAsync();
            var responses = await _context.CampaignResponses.ToListAsync();
            var csv = new StringBuilder();

            csv.AppendLine("Customer ID,Age,Income,Education,Country,Customer Segment,Average Spend,Purchases,Response Rate");

            var customerStats = responses.GroupBy(r => r.CustomerId).ToDictionary(
                g => g.Key,
                g => new {
                    YesCount = g.Count(r => r.Response == "Yes"),
                    ResponseCount = g.Count(),
                    TotalSpend = g.Where(r => r.Response == "Yes").Sum(r => r.PurchaseAmount),
                    TotalPurchases = g.Where(r => r.Response == "Yes").Sum(r => r.NumberOfPurchases)
                }
            );

            int totalCust = customers.Count;
            decimal avgSpend = totalCust > 0 ? responses.Where(r => r.Response == "Yes").Sum(r => r.PurchaseAmount) / totalCust : 0m;
            decimal highThreshold = avgSpend * 1.2m;
            decimal lowThreshold = avgSpend * 0.5m;

            foreach (var c in customers)
            {
                var stats = customerStats.GetValueOrDefault(c.CustomerId);
                decimal monetary = stats?.TotalSpend ?? 0m;
                int frequency = stats?.TotalPurchases ?? 0;
                double responseRate = stats?.ResponseCount > 0 ? (double)stats.YesCount / stats.ResponseCount : 0.0;

                string segment = "Medium Value";
                if (monetary >= highThreshold) segment = "High Value";
                else if (monetary < lowThreshold) segment = "Low Value";

                double avgPurchaseSpend = frequency > 0 ? (double)monetary / frequency : 0.0;

                csv.AppendLine($"{c.CustomerId},{c.Age},{c.Income:F2},{EscapeCsv(c.Education)},{EscapeCsv(c.Country)},{EscapeCsv(segment)},{avgPurchaseSpend:F2},{frequency},{responseRate:F4}");
            }

            string filePath = Path.Combine(GetExportDirectory(), "customer_analytics.csv");
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Exports overall campaign summary KPIs to Tableau/Datasets/campaign_summary.csv.
        /// </summary>
        public async Task<string> ExportCampaignSummaryAsync()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var responses = await _context.CampaignResponses.ToListAsync();

            decimal totalRevenue = campaigns.Sum(c => c.Revenue);
            decimal totalSpend = campaigns.Sum(c => c.Spend);
            double avgRoi = totalSpend > 0 ? (double)(totalRevenue - totalSpend) / (double)totalSpend : 0.0;

            int totalResponsesCount = responses.Count;
            int yesResponsesCount = responses.Count(r => r.Response == "Yes");
            double averageResponseRate = totalResponsesCount > 0 ? (double)yesResponsesCount / totalResponsesCount : 0.0;

            string bestCampaign = campaigns
                .OrderByDescending(c => c.Spend > 0 ? (c.Revenue - c.Spend) / c.Spend : 0)
                .FirstOrDefault()?.CampaignName ?? "N/A";

            string bestChannel = campaigns
                .GroupBy(c => c.MarketingChannel)
                .OrderByDescending(g => g.Sum(c => c.Spend) > 0 ? (g.Sum(c => c.Revenue) - g.Sum(c => c.Spend)) / g.Sum(c => c.Spend) : 0)
                .FirstOrDefault()?.Key ?? "N/A";

            var csv = new StringBuilder();
            csv.AppendLine("Campaign KPIs,Value");
            csv.AppendLine($"Total Revenue,{totalRevenue:F2}");
            csv.AppendLine($"Total Spend,{totalSpend:F2}");
            csv.AppendLine($"Average ROI,{avgRoi:F4}");
            csv.AppendLine($"Average Response Rate,{averageResponseRate:F4}");
            csv.AppendLine($"Best Campaign,{EscapeCsv(bestCampaign)}");
            csv.AppendLine($"Best Channel,{EscapeCsv(bestChannel)}");

            string filePath = Path.Combine(GetExportDirectory(), "campaign_summary.csv");
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Exports monthly revenue and spends trend to Tableau/Datasets/monthly_revenue.csv.
        /// </summary>
        public async Task<string> ExportMonthlyRevenueAsync()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var csv = new StringBuilder();

            csv.AppendLine("Month,Revenue,Campaign Spend,ROI");

            var monthlyData = campaigns
                .GroupBy(c => c.StartDate.ToString("yyyy-MM"))
                .Select(g => new {
                    Month = g.Key,
                    Revenue = g.Sum(c => c.Revenue),
                    Spend = g.Sum(c => c.Spend),
                    Roi = g.Sum(c => c.Spend) > 0 ? (double)(g.Sum(c => c.Revenue) - g.Sum(c => c.Spend)) / (double)g.Sum(c => c.Spend) : 0.0
                })
                .OrderBy(x => x.Month)
                .ToList();

            foreach (var m in monthlyData)
            {
                csv.AppendLine($"{m.Month},{m.Revenue:F2},{m.Spend:F2},{m.Roi:F4}");
            }

            string filePath = Path.Combine(GetExportDirectory(), "monthly_revenue.csv");
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Triggers all CSV exports together.
        /// </summary>
        public async Task<string> ExportAllAsync()
        {
            await ExportCampaignPerformanceAsync();
            await ExportCustomerAnalyticsAsync();
            await ExportCampaignSummaryAsync();
            await ExportMonthlyRevenueAsync();
            return GetExportDirectory();
        }

        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
