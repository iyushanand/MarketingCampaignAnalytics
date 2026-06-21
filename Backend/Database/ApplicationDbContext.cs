using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Database
{
    /// <summary>
    /// Database context for the Marketing Campaign Analytics application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The context options.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Campaigns DB set.
        /// </summary>
        public DbSet<Campaign> Campaigns { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Customers DB set.
        /// </summary>
        public DbSet<Customer> Customers { get; set; } = null!;

        /// <summary>
        /// Gets or sets the CampaignResponses DB set.
        /// </summary>
        public DbSet<CampaignResponse> CampaignResponses { get; set; } = null!;

        /// <summary>
        /// Applies Fluent API database schema configurations.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configure Decimal precisions
            modelBuilder.Entity<Campaign>()
                .Property(c => c.Budget)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Campaign>()
                .Property(c => c.Spend)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Campaign>()
                .Property(c => c.Revenue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Customer>()
                .Property(c => c.Income)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CampaignResponse>()
                .Property(r => r.PurchaseAmount)
                .HasColumnType("decimal(18,2)");

            // 2. Configure Relationships via Fluent API
            modelBuilder.Entity<CampaignResponse>()
                .HasOne(r => r.Campaign)
                .WithMany(c => c.CampaignResponses)
                .HasForeignKey(r => r.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CampaignResponse>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.CampaignResponses)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Configure Indexes for performance
            modelBuilder.Entity<Campaign>()
                .HasIndex(c => c.CampaignName)
                .HasDatabaseName("IX_Campaign_CampaignName");

            modelBuilder.Entity<Campaign>()
                .HasIndex(c => c.MarketingChannel)
                .HasDatabaseName("IX_Campaign_MarketingChannel");

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Country)
                .HasDatabaseName("IX_Customer_Country");

            modelBuilder.Entity<CampaignResponse>()
                .HasIndex(r => r.PurchaseDate)
                .HasDatabaseName("IX_CampaignResponse_PurchaseDate");

            modelBuilder.Entity<CampaignResponse>()
                .HasIndex(r => r.Response)
                .HasDatabaseName("IX_CampaignResponse_Response");
        }
    }
}
