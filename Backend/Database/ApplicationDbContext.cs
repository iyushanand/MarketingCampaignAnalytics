using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<CampaignResponse> CampaignResponses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure precision for decimal types if required
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
        }
    }
}
