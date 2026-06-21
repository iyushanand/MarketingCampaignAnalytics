using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves customer demographics, profile data, summaries, and value segmentation details.
    /// </summary>
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        /// <summary>
        /// Gets all customers with details, RFM metrics, and segment tags.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetCustomersAsync();
            return Ok(ApiResponse<IEnumerable<CustomerDto>>.Ok(customers));
        }

        /// <summary>
        /// Gets detailed attributes for a single customer.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.Fail($"Customer with ID {id} not found."));
            }
            return Ok(ApiResponse<CustomerDto>.Ok(customer));
        }

        /// <summary>
        /// Gets consolidated KPIs and behavior metrics.
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetCustomerSummary()
        {
            var summary = await _customerService.GetCustomerSummaryAsync();
            return Ok(ApiResponse<CustomerAnalyticsDto>.Ok(summary));
        }

        /// <summary>
        /// Gets rule-based customer personas.
        /// </summary>
        [HttpGet("personas")]
        public async Task<IActionResult> GetCustomerPersonas()
        {
            var personas = await _customerService.GetCustomerPersonasAsync();
            return Ok(ApiResponse<IEnumerable<CustomerPersonaDto>>.Ok(personas));
        }

        /// <summary>
        /// Gets demographic distributions and cross-sectional analysis.
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetCustomerAnalytics()
        {
            var analytics = await _customerService.GetCustomerAnalyticsAsync();
            return Ok(ApiResponse<CustomerDemographicsDto>.Ok(analytics));
        }
    }
}
