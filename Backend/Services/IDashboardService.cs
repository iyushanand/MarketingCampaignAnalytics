using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    public interface IDashboardService
    {
        Task<DashboardKpisDto> GetDashboardKpisAsync();
    }
}
