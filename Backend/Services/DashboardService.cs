using System;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.Models;
using Backend.Repository;

namespace Backend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CampaignResponse> _responseRepository;

        public DashboardService(
            IRepository<Campaign> campaignRepository,
            IRepository<Customer> customerRepository,
            IRepository<CampaignResponse> responseRepository)
        {
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
        }

        public async Task<DashboardKpisDto> GetDashboardKpisAsync()
        {
            // Placeholder: Returns default KPIs. Implementation in Phase 5.
            return await Task.FromResult(new DashboardKpisDto());
        }
    }
}
