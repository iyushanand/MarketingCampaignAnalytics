using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Database
{
    /// <summary>
    /// Handles database migrations and preloading the Kaggle dataset.
    /// </summary>
    public static class DbInitializer
    {
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
        /// Check for pending migrations and apply them automatically.
        /// </summary>
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }

        /// <summary>
        /// Loads the Kaggle Customer Personality Analysis CSV and populates SQL Server database.
        /// </summary>
        public static async Task<bool> LoadSampleDataset(ApplicationDbContext context, string csvPath)
        {
            try
            {
                // 1. Check if already seeded
                if (await context.Customers.AnyAsync())
                {
                    return true; // Already loaded
                }

                if (!File.Exists(csvPath))
                {
                    throw new FileNotFoundException("Kaggle CSV dataset file not found.", csvPath);
                }

                // 2. Add Campaigns
                var campaigns = GetDefaultCampaigns();
                await context.Campaigns.AddRangeAsync(campaigns);
                await context.SaveChangesAsync();

                // 3. Parse CSV and Add Customers and Responses
                var lines = await File.ReadAllLinesAsync(csvPath);
                if (lines.Length <= 1) return false; // Empty file or header only

                var random = new Random();
                var customersList = new List<Customer>();
                var responsesList = new List<CampaignResponse>();

                // Parse header to map columns (Kaggle dataset uses tab separator "\t")
                var header = lines[0].Split('\t');
                var colIndices = GetColumnIndices(header);

                // Batch insert variables
                int batchSize = 250;
                int currentCount = 0;

                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('\t');
                    if (parts.Length < header.Length) continue;

                    // Map Customer
                    int birthYear = ParseIntOrDefault(parts, colIndices["Year_Birth"], 1980);
                    int age = 2026 - birthYear; // Based on 2026 system metadata year
                    decimal income = ParseDecimalOrDefault(parts, colIndices["Income"], 0);
                    string education = parts[colIndices["Education"]].Trim();
                    string maritalStatus = parts[colIndices["Marital_Status"]].Trim();

                    // Generate mock identifiers
                    string firstName = FirstNames[random.Next(FirstNames.Length)];
                    string lastName = LastNames[random.Next(LastNames.Length)];
                    string gender = random.Next(2) == 0 ? "Male" : "Female";
                    string country = Countries[random.Next(Countries.Length)];
                    var cities = CitiesByCountry[country];
                    string city = cities[random.Next(cities.Length)];

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

                    // Pre-generate response details
                    int recency = ParseIntOrDefault(parts, colIndices["Recency"], 0);
                    decimal totalSpent = 
                        ParseDecimalOrDefault(parts, colIndices["MntWines"], 0) +
                        ParseDecimalOrDefault(parts, colIndices["MntFruits"], 0) +
                        ParseDecimalOrDefault(parts, colIndices["MntMeatProducts"], 0) +
                        ParseDecimalOrDefault(parts, colIndices["MntFishProducts"], 0) +
                        ParseDecimalOrDefault(parts, colIndices["MntSweetProducts"], 0) +
                        ParseDecimalOrDefault(parts, colIndices["MntGoldProds"], 0);

                    int totalPurchases =
                        ParseIntOrDefault(parts, colIndices["NumWebPurchases"], 0) +
                        ParseIntOrDefault(parts, colIndices["NumCatalogPurchases"], 0) +
                        ParseIntOrDefault(parts, colIndices["NumStorePurchases"], 0);

                    // Track campaign acceptance fields in Kaggle schema
                    int acceptedCmp1 = ParseIntOrDefault(parts, colIndices["AcceptedCmp1"], 0);
                    int acceptedCmp2 = ParseIntOrDefault(parts, colIndices["AcceptedCmp2"], 0);
                    int acceptedCmp3 = ParseIntOrDefault(parts, colIndices["AcceptedCmp3"], 0);
                    int acceptedCmp4 = ParseIntOrDefault(parts, colIndices["AcceptedCmp4"], 0);
                    int acceptedCmp5 = ParseIntOrDefault(parts, colIndices["AcceptedCmp5"], 0);
                    int acceptedCmpLast = ParseIntOrDefault(parts, colIndices["Response"], 0);

                    // Prepare CampaignResponses list (will link once customer gets an ID)
                    var userResponses = new[]
                    {
                        new { Index = 0, Accepted = acceptedCmp1 },
                        new { Index = 1, Accepted = acceptedCmp2 },
                        new { Index = 2, Accepted = acceptedCmp3 },
                        new { Index = 3, Accepted = acceptedCmp4 },
                        new { Index = 4, Accepted = acceptedCmp5 },
                        new { Index = 5, Accepted = acceptedCmpLast }
                    };

                    // We batch insert customers to obtain their auto-generated CustomerIds
                    if (currentCount >= batchSize || i == lines.Length - 1)
                    {
                        await context.Customers.AddRangeAsync(customersList);
                        await context.SaveChangesAsync();

                        // Now link responses
                        for (int j = 0; j < customersList.Count; j++)
                        {
                            var savedCustomer = customersList[j];
                            
                            // Map the customer responses to our 6 campaigns
                            for (int k = 0; k < 6; k++)
                            {
                                var respDef = userResponses[k];
                                var campaign = campaigns[k];
                                bool accepted = respDef.Accepted == 1;

                                decimal purchaseAmount = 0;
                                int purchaseCount = 0;

                                if (accepted)
                                {
                                    purchaseAmount = Math.Round(totalSpent / (random.Next(2, 4)), 2);
                                    purchaseCount = Math.Max(1, totalPurchases / (random.Next(2, 4)));
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

                        await context.CampaignResponses.AddRangeAsync(responsesList);
                        await context.SaveChangesAsync();

                        // Clear lists for next batch
                        customersList.Clear();
                        responsesList.Clear();
                        currentCount = 0;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        private static Dictionary<string, int> GetColumnIndices(string[] header)
        {
            var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Length; i++)
            {
                indices[header[i].Trim()] = i;
            }
            return indices;
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
