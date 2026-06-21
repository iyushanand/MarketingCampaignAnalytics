using System;
using System.IO;
using System.Threading.Tasks;
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
        /// Placeholder for custom CSV uploads. Will be completed in Phase 3.
        /// </summary>
        public async Task<bool> UploadCsvAsync(Stream fileStream)
        {
            // Placeholder: Custom file parsing. Implementation in Phase 3 / Upload API.
            await Task.CompletedTask;
            return true;
        }
    }
}
