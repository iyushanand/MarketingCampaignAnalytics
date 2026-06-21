using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for customer analytics and RFM analysis.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Gets all customers with details, RFM metrics, and segment tags.
        /// </summary>
        Task<IEnumerable<CustomerDto>> GetCustomersAsync();

        /// <summary>
        /// Gets a customer by their ID.
        /// </summary>
        Task<CustomerDto?> GetCustomerByIdAsync(int id);

        /// <summary>
        /// Gets consolidated KPIs and behavior metrics.
        /// </summary>
        Task<CustomerAnalyticsDto> GetCustomerSummaryAsync();

        /// <summary>
        /// Gets rule-based customer personas.
        /// </summary>
        Task<IEnumerable<CustomerPersonaDto>> GetCustomerPersonasAsync();

        /// <summary>
        /// Gets demographic distributions and cross-sectional analysis.
        /// </summary>
        Task<CustomerDemographicsDto> GetCustomerAnalyticsAsync();
    }
}
