using Backend.Database;
using Backend.Models;

namespace Backend.Repository
{
    /// <summary>
    /// Repository implementation for Campaign entity operations.
    /// </summary>
    public class CampaignRepository : Repository<Campaign>, ICampaignRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CampaignRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
