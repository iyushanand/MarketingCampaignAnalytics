using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Database;
using Backend.Models;
using Backend.Repository;

namespace Backend.Services
{
    /// <summary>
    /// Service implementation for dataset uploads and seeding.
    /// </summary>
    public class UploadService : IUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignResponse> _responseRepository;

        private static readonly string[] FirstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Elizabeth", "William", "Linda", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
        private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Wilson", "Martinez", "Anderson", "Taylor", "Thomas", "Hernandez", "Moore", "Martin", "Jackson", "Martin", "Lee" };
        private static readonly string[] Countries = { "United States", "Canada", "United Kingdom", "Australia", "Germany" };
        private static readonly Dictionary<string, string[]> CitiesByCountry = new()
        {
            { "United States", new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix" } },
            { "Canada", new[] { "Toronto", "Montreal", "Vancouver", "Calgary", "Ottawa" } },
            { "United Kingdom", new[] { "London", "Birmingham", "Leeds", "Glasgow", "Sheffield" } },
            { "Australia", new[] { "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide" } },
            { "Germany", new[] { "Berlin", "Hamburg", "Munich", "Cologne", "Frankfurt" } }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadService"/> class.
        /// </summary>
        public UploadService(
            ApplicationDbContext context,
            IRepository<Customer> customerRepository,
            IRepository<Campaign> campaignRepository,
            IRepository<CampaignResponse> responseRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
        }

        /// <summary>
        /// Loads the bundled Kaggle Customer Personality dataset into the database.
        /// </summary>
        public async Task<bool> LoadSampleDatasetAsync()
        {
            string[] possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "data", "marketing_campaign.csv"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "marketing_campaign.csv"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "marketing_campaign.csv"),
                Path.Combine(Directory.GetCurrentDirectory(), "data", "marketing_campaign.csv"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "marketing_campaign.csv"),
                @"C:\Users\KIIT\.gemini\antigravity\scratch\MarketingCampaignAnalytics\data\marketing_campaign.csv"
            };

            string csvPath = string.Empty;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    csvPath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(csvPath))
            {
                throw new FileNotFoundException("Kaggle Customer Personality Analysis CSV file could not be resolved.");
            }

            return await DbInitializer.LoadSampleDataset(_context, csvPath);
        }

        /// <summary>
        /// Custom CSV upload, validation, parsing, and database storage.
        /// </summary>
        public async Task<bool> UploadCsvAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var linesList = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    linesList.Add(line);
                }
            }

            if (linesList.Count <= 1) return false;

            // Detect separator (tab or comma)
            string firstLine = linesList[0];
            char separator = '\t';
            if (firstLine.Contains(","))
            {
                separator = ',';
            }

            var header = firstLine.Split(separator).Select(h => h.Trim()).ToArray();
            
            // Validate required columns
            var requiredCols = new[] { "Year_Birth", "Income", "Education", "Marital_Status" };
            foreach (var colName in requiredCols)
            {
                if (!header.Contains(colName, StringComparer.OrdinalIgnoreCase))
                {
                    return false; // Missing crucial demographics
                }
            }

            // Clean database tables to load new file
            _context.CampaignResponses.RemoveRange(_context.CampaignResponses);
            _context.Customers.RemoveRange(_context.Customers);
            _context.Campaigns.RemoveRange(_context.Campaigns);
            await _context.SaveChangesAsync();

            // Re-seed campaigns
            var campaigns = GetDefaultCampaigns();
            await _context.Campaigns.AddRangeAsync(campaigns);
            await _context.SaveChangesAsync();

            var colIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int k = 0; k < header.Length; k++)
            {
                colIndices[header[k]] = k;
            }

            var random = new Random();
            var customersList = new List<Customer>();
            var responsesList = new List<CampaignResponse>();
            int batchSize = 250;
            int currentCount = 0;

            for (int i = 1; i < linesList.Count; i++)
            {
                var line = linesList[i];
                var parts = line.Split(separator);
                if (parts.Length < header.Length) continue;

                int birthYear = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("Year_Birth", -1), 1980);
                int age = 2026 - birthYear;
                decimal income = ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("Income", -1), 0);
                string education = parts[colIndices.GetValueOrDefault("Education", -1)].Trim();
                string maritalStatus = parts[colIndices.GetValueOrDefault("Marital_Status", -1)].Trim();

                string firstName = FirstNames[random.Next(FirstNames.Length)];
                string lastName = LastNames[random.Next(LastNames.Length)];
                string gender = random.Next(2) == 0 ? "Male" : "Female";
                string country = Countries[random.Next(Countries.Length)];
                string city = CitiesByCountry[country][random.Next(CitiesByCountry[country].Length)];

                var customer = new Customer
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = gender,
                    Age = age,
                    Income = income,
                    Education = education,
                    MaritalStatus = maritalStatus,
                    Country = country,
                    City = city,
                    CreatedAt = DateTime.UtcNow
                };

                customersList.Add(customer);
                currentCount++;

                decimal totalSpent = 
                    ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("MntWines", -1), 0) +
                    ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("MntFruits", -1), 0) +
                    ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("MntMeatProducts", -1), 0) +
                    ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("MntFishProducts", -1), 0) +
                    ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("MntSweetProducts", -1), 0) +
                    ParseDecimalOrDefault(parts, colIndices.GetValueOrDefault("MntGoldProds", -1), 0);

                int totalPurchases =
                    ParseIntOrDefault(parts, colIndices.GetValueOrDefault("NumWebPurchases", -1), 0) +
                    ParseIntOrDefault(parts, colIndices.GetValueOrDefault("NumCatalogPurchases", -1), 0) +
                    ParseIntOrDefault(parts, colIndices.GetValueOrDefault("NumStorePurchases", -1), 0);

                int acceptedCmp1 = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("AcceptedCmp1", -1), 0);
                int acceptedCmp2 = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("AcceptedCmp2", -1), 0);
                int acceptedCmp3 = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("AcceptedCmp3", -1), 0);
                int acceptedCmp4 = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("AcceptedCmp4", -1), 0);
                int acceptedCmp5 = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("AcceptedCmp5", -1), 0);
                int acceptedCmpLast = ParseIntOrDefault(parts, colIndices.GetValueOrDefault("Response", -1), 0);

                var userResponses = new[]
                {
                    new { Accepted = acceptedCmp1 },
                    new { Accepted = acceptedCmp2 },
                    new { Accepted = acceptedCmp3 },
                    new { Accepted = acceptedCmp4 },
                    new { Accepted = acceptedCmp5 },
                    new { Accepted = acceptedCmpLast }
                };

                if (currentCount >= batchSize || i == linesList.Count - 1)
                {
                    await _context.Customers.AddRangeAsync(customersList);
                    await _context.SaveChangesAsync();

                    for (int j = 0; j < customersList.Count; j++)
                    {
                        var savedCustomer = customersList[j];

                        for (int k = 0; k < 6; k++)
                        {
                            var respDef = userResponses[k];
                            var campaign = campaigns[k];
                            bool accepted = respDef.Accepted == 1;

                            decimal purchaseAmount = 0;
                            int purchaseCount = 0;

                            if (accepted)
                            {
                                purchaseAmount = Math.Round(totalSpent / random.Next(2, 4), 2);
                                purchaseCount = Math.Max(1, totalPurchases / random.Next(2, 4));
                            }

                            var campaignResponse = new CampaignResponse
                            {
                                CustomerId = savedCustomer.CustomerId,
                                CampaignId = campaign.CampaignId,
                                Response = accepted ? "Yes" : "No",
                                PurchaseAmount = purchaseAmount,
                                PurchaseDate = campaign.StartDate.AddDays(random.Next(1, 20)),
                                NumberOfPurchases = purchaseCount,
                                CreatedAt = DateTime.UtcNow
                            };
                            responsesList.Add(campaignResponse);
                        }
                    }

                    await _context.CampaignResponses.AddRangeAsync(responsesList);
                    await _context.SaveChangesAsync();

                    customersList.Clear();
                    responsesList.Clear();
                    currentCount = 0;
                }
            }

            return true;
        }

        private static List<Campaign> GetDefaultCampaigns()
        {
            return new List<Campaign>
            {
                new() { CampaignName = "Acquisition Campaign 1", CampaignType = "Acquisition", MarketingChannel = "Email", Budget = 50000, Spend = 48000, Revenue = 75000, Conversions = 150, Clicks = 1500, Impressions = 30000, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 2, 1), Status = "Completed" },
                new() { CampaignName = "Acquisition Campaign 2", CampaignType = "Acquisition", MarketingChannel = "SMS", Budget = 30000, Spend = 28000, Revenue = 45000, Conversions = 90, Clicks = 900, Impressions = 18000, StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 3, 1), Status = "Completed" },
                new() { CampaignName = "Retention Campaign 3", CampaignType = "Retention", MarketingChannel = "Social Media", Budget = 60000, Spend = 58000, Revenue = 90000, Conversions = 220, Clicks = 2400, Impressions = 48000, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 4, 1), Status = "Completed" },
                new() { CampaignName = "Retention Campaign 4", CampaignType = "Retention", MarketingChannel = "Google Search Ads", Budget = 40000, Spend = 39000, Revenue = 65000, Conversions = 130, Clicks = 1200, Impressions = 24000, StartDate = new DateTime(2026, 4, 1), EndDate = new DateTime(2026, 5, 1), Status = "Completed" },
                new() { CampaignName = "Loyalty Campaign 5", CampaignType = "Loyalty", MarketingChannel = "Display Ads", Budget = 45000, Spend = 44000, Revenue = 55000, Conversions = 110, Clicks = 1100, Impressions = 22000, StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 6, 1), Status = "Completed" },
                new() { CampaignName = "Promo Final Campaign", CampaignType = "Acquisition", MarketingChannel = "Email", Budget = 80000, Spend = 78000, Revenue = 130000, Conversions = 320, Clicks = 3200, Impressions = 64000, StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 7, 1), Status = "Completed" }
            };
        }

        private static int ParseIntOrDefault(string[] parts, int index, int defaultValue)
        {
            if (index < 0 || index >= parts.Length) return defaultValue;
            return int.TryParse(parts[index], out var val) ? val : defaultValue;
        }

        private static decimal ParseDecimalOrDefault(string[] parts, int index, decimal defaultValue)
        {
            if (index < 0 || index >= parts.Length) return defaultValue;
            return decimal.TryParse(parts[index], out var val) ? val : defaultValue;
        }
    }
}
