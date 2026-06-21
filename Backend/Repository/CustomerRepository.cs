using Backend.Database;
using Backend.Models;

namespace Backend.Repository
{
    /// <summary>
    /// Repository implementation for Customer entity operations.
    /// </summary>
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
