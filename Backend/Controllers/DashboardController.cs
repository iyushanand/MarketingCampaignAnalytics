using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetDashboardKpis()
        {
            var kpis = await _dashboardService.GetDashboardKpisAsync();
            return Ok(kpis);
        }
    }
}
