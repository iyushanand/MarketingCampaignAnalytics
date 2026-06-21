using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [HttpGet("insights")]
        public async Task<IActionResult> GetCustomerInsights()
        {
            var insights = await _customerService.GetCustomerInsightsAsync();
            return Ok(insights);
        }
    }
}
