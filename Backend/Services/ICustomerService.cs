using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for customer demographics and RFM analysis.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Gets all customers.
        /// </summary>
        Task<IEnumerable<CustomerDto>> GetCustomersAsync();

        /// <summary>
        /// Gets a customer by their ID.
        /// </summary>
        Task<CustomerDto?> GetCustomerByIdAsync(int id);

        /// <summary>
        /// Gets customer insights (age, income, RFM lists).
        /// </summary>
        Task<CustomerInsightsDto> GetCustomerInsightsAsync();

        /// <summary>
        /// Gets demographics distributions (Age, Gender, Income, Country).
        /// </summary>
        Task<CustomerDemographicsDto> GetCustomerDemographicsAsync();

        /// <summary>
        /// Calculates customer RFM segments (High, Medium, Low) using percentile-based logic.
        /// </summary>
        Task<List<RfmSegmentDto>> GetCustomerRfmTiersAsync();
    }
}
