using Backend.Database;
using Backend.Models;

namespace Backend.Repository
{
    /// <summary>
    /// Repository implementation for CampaignResponse entity operations.
    /// </summary>
    public class CampaignResponseRepository : Repository<CampaignResponse>, ICampaignResponseRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignResponseRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CampaignResponseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
