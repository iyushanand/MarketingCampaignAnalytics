using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers
{
    /// <summary>
    /// Serves customer demographics, profile data and RFM value segmentation details.
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
        /// Gets a paginated or standard list of customers.
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
        /// Gets country, gender, age and income distributions.
        /// </summary>
        [HttpGet("demographics")]
        public async Task<IActionResult> GetCustomerDemographics()
        {
            var demographics = await _customerService.GetCustomerDemographicsAsync();
            return Ok(ApiResponse<CustomerDemographicsDto>.Ok(demographics));
        }

        /// <summary>
        /// Gets RFM Value segments (High, Medium, Low value customers) using percentiles.
        /// </summary>
        [HttpGet("rfm")]
        public async Task<IActionResult> GetCustomerRfm()
        {
            var rfm = await _customerService.GetCustomerRfmTiersAsync();
            return Ok(ApiResponse<IEnumerable<RfmSegmentDto>>.Ok(rfm));
        }
    }
}
