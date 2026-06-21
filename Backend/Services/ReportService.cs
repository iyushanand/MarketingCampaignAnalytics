using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Database;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service implementation for report generation and archive log management.
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
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            string rootDir = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(Path.Combine(rootDir, "Reports", "Excel"));
            Directory.CreateDirectory(Path.Combine(rootDir, "Reports", "PDF"));
        }

        /// <summary>
        /// Generates a professional Excel report, saves it to disk, and returns the physical path.
        /// </summary>
        public async Task<string> GenerateExcelReportAsync()
        {
            string tempJsonPath = Path.Combine(Path.GetTempPath(), $"report_data_{Guid.NewGuid()}.json");
            try
            {
                var reportData = await CompileReportDataAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string jsonString = JsonSerializer.Serialize(reportData, jsonOptions);
                await File.WriteAllTextAsync(tempJsonPath, jsonString);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Marketing_Report_{timestamp}.xlsx";
                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Excel", fileName);

                // Run Python report generator
                await _pythonRunner.RunScriptAsync("report_generator.py", "excel", outputPath, tempJsonPath);

                return outputPath;
            }
            finally
            {
                if (File.Exists(tempJsonPath))
                {
                    File.Delete(tempJsonPath);
                }
            }
        }

        /// <summary>
        /// Generates a professional PDF report, saves it to disk, and returns the physical path.
        /// </summary>
        public async Task<string> GeneratePdfReportAsync()
        {
            string tempJsonPath = Path.Combine(Path.GetTempPath(), $"report_data_{Guid.NewGuid()}.json");
            try
            {
                var reportData = await CompileReportDataAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string jsonString = JsonSerializer.Serialize(reportData, jsonOptions);
                await File.WriteAllTextAsync(tempJsonPath, jsonString);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Marketing_Report_{timestamp}.pdf";
                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "PDF", fileName);

                // Run Python report generator
                await _pythonRunner.RunScriptAsync("report_generator.py", "pdf", outputPath, tempJsonPath);

                return outputPath;
            }
            finally
            {
                if (File.Exists(tempJsonPath))
                {
                    File.Delete(tempJsonPath);
                }
            }
        }

        /// <summary>
        /// Retrieves metadata for all previously generated Excel and PDF reports.
        /// </summary>
        public async Task<IEnumerable<ReportFileDto>> GetGeneratedReportsAsync()
        {
            var reports = new List<ReportFileDto>();
            string rootDir = Directory.GetCurrentDirectory();

            // Scan Excel Folder
            string excelDir = Path.Combine(rootDir, "Reports", "Excel");
            if (Directory.Exists(excelDir))
            {
                var files = Directory.GetFiles(excelDir, "*.xlsx");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    reports.Add(new ReportFileDto
                    {
                        FileName = fileInfo.Name,
                        FileType = "Excel",
                        FileSize = FormatBytes(fileInfo.Length),
                        CreatedAt = fileInfo.CreationTime,
                        DownloadUrl = $"/api/reports/download?fileName={Uri.EscapeDataString(fileInfo.Name)}"
                    });
                }
            }

            // Scan PDF Folder
            string pdfDir = Path.Combine(rootDir, "Reports", "PDF");
            if (Directory.Exists(pdfDir))
            {
                var files = Directory.GetFiles(pdfDir, "*.pdf");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    reports.Add(new ReportFileDto
                    {
                        FileName = fileInfo.Name,
                        FileType = "PDF",
                        FileSize = FormatBytes(fileInfo.Length),
                        CreatedAt = fileInfo.CreationTime,
                        DownloadUrl = $"/api/reports/download?fileName={Uri.EscapeDataString(fileInfo.Name)}"
                    });
                }
            }

            return await Task.FromResult(reports.OrderByDescending(r => r.CreatedAt).ToList());
        }

        /// <summary>
        /// Compiles database analytics data into a dynamic dictionary structure.
        /// </summary>
        private async Task<Dictionary<string, object>> CompileReportDataAsync()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            var responses = await _context.CampaignResponses.ToListAsync();
            var customers = await _context.Customers.ToListAsync();

            // 1. Executive Summary calculations
            decimal totalRevenue = campaigns.Sum(c => c.Revenue);
            decimal totalSpend = campaigns.Sum(c => c.Spend);
            double roi = totalSpend > 0 ? (double)(totalRevenue - totalSpend) / (double)totalSpend * 100.0 : 0.0;

            int totalResponsesCount = responses.Count;
            int yesResponsesCount = responses.Count(r => r.Response == "Yes");
            double averageResponseRate = totalResponsesCount > 0 ? (double)yesResponsesCount / totalResponsesCount * 100.0 : 0.0;

            int totalImpressions = campaigns.Sum(c => c.Impressions);
            int totalConversions = campaigns.Sum(c => c.Conversions);
            double averageConversionRate = totalImpressions > 0 ? (double)totalConversions / totalImpressions * 100.0 : 0.0;

            string bestCampaign = campaigns
                .OrderByDescending(c => c.Spend > 0 ? (c.Revenue - c.Spend) / c.Spend : 0)
                .FirstOrDefault()?.CampaignName ?? "N/A";

            string bestChannel = campaigns
                .GroupBy(c => c.MarketingChannel)
                .OrderByDescending(g => g.Sum(c => c.Spend) > 0 ? (g.Sum(c => c.Revenue) - g.Sum(c => c.Spend)) / g.Sum(c => c.Spend) : 0)
                .FirstOrDefault()?.Key ?? "N/A";

            var executiveSummary = new Dictionary<string, object>
            {
                { "totalRevenue", (double)totalRevenue },
                { "totalSpend", (double)totalSpend },
                { "roi", roi },
                { "averageResponseRate", averageResponseRate },
                { "averageConversionRate", averageConversionRate },
                { "bestCampaign", bestCampaign },
                { "bestChannel", bestChannel }
            };

            // Group campaign responses by CampaignId in-memory to prevent N+1 queries
            var responsesByCampaign = responses.GroupBy(r => r.CampaignId).ToDictionary(
                g => g.Key,
                g => new {
                    TotalCount = g.Count(),
                    YesCount = g.Count(r => r.Response == "Yes")
                }
            );

            // 2. Campaign Performance detailed view
            var campaignPerformance = campaigns.Select(c => {
                var stats = responsesByCampaign.GetValueOrDefault(c.CampaignId);
                double responseRate = stats != null && stats.TotalCount > 0
                    ? (double)stats.YesCount / stats.TotalCount * 100.0
                    : 0.0;

                return new Dictionary<string, object>
                {
                    { "campaignName", c.CampaignName },
                    { "channel", c.MarketingChannel },
                    { "spend", (double)c.Spend },
                    { "revenue", (double)c.Revenue },
                    { "roi", c.Spend > 0 ? (double)(c.Revenue - c.Spend) / (double)c.Spend * 100.0 : 0.0 },
                    { "roas", c.Spend > 0 ? (double)c.Revenue / (double)c.Spend : 0.0 },
                    { "conversions", c.Conversions },
                    { "responseRate", responseRate }
                };
            }).ToList();

            // 3. Customer Analytics RFM summary mapping
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

            var customerSegmentsList = customers.Select(c => {
                var stats = customerStats.GetValueOrDefault(c.CustomerId);
                decimal monetary = stats?.TotalSpend ?? 0m;
                int frequency = stats?.TotalPurchases ?? 0;
                double responseRate = stats?.ResponseCount > 0 ? (double)stats.YesCount / stats.ResponseCount * 100.0 : 0.0;
                
                string segment = "Medium Value";
                if (monetary >= highThreshold) segment = "High Value";
                else if (monetary < lowThreshold) segment = "Low Value";
                
                return new {
                    Segment = segment,
                    Monetary = monetary,
                    Purchases = frequency,
                    Income = c.Income,
                    ResponseRate = responseRate
                };
            }).ToList();

            var customerAnalytics = customerSegmentsList.GroupBy(s => s.Segment).Select(g => new Dictionary<string, object>
            {
                { "segment", g.Key },
                { "customerCount", g.Count() },
                { "averageSpend", (double)(g.Any() ? Math.Round(g.Average(x => x.Monetary), 2) : 0m) },
                { "averageIncome", (double)(g.Any() ? Math.Round(g.Average(x => x.Income), 2) : 0m) },
                { "averagePurchases", g.Any() ? Math.Round(g.Average(x => x.Purchases), 1) : 0.0 },
                { "responseRate", g.Any() ? Math.Round(g.Average(x => x.ResponseRate), 2) : 0.0 }
            }).ToList();

            // 4. Monthly Revenue summary
            var monthlyRevenue = campaigns
                .GroupBy(c => c.StartDate.ToString("yyyy-MM"))
                .Select(g => new Dictionary<string, object>
                {
                    { "month", g.Key },
                    { "revenue", (double)g.Sum(c => c.Revenue) },
                    { "spend", (double)g.Sum(c => c.Spend) },
                    { "conversions", g.Sum(c => c.Conversions) },
                    { "roi", g.Sum(c => c.Spend) > 0 
                        ? (double)(g.Sum(c => c.Revenue) - g.Sum(c => c.Spend)) / (double)g.Sum(c => c.Spend) * 100.0 
                        : 0.0 }
                })
                .OrderBy(x => (string)x["month"])
                .ToList();

            return new Dictionary<string, object>
            {
                { "executiveSummary", executiveSummary },
                { "campaignPerformance", campaignPerformance },
                { "customerAnalytics", customerAnalytics },
                { "monthlyRevenue", monthlyRevenue }
            };
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB" };
            double dblSized = bytes;
            int i = 0;
            while (dblSized >= 1024 && i < suffix.Length - 1)
            {
                dblSized /= 1024;
                i++;
            }
            return $"{dblSized:0.1} {suffix[i]}";
        }

        /// <summary>
        /// Predefined legacy JSON-based reporting
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
                        .Take(500)
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
    }
}
