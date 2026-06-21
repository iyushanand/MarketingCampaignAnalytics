using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    public interface ICustomerService
    {
        Task<CustomerInsightsDto> GetCustomerInsightsAsync();
    }
}
