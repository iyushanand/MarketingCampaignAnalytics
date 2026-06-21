using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.DTOs;
using Backend.Models;
using Backend.Repository;

namespace Backend.Services
{
    /// <summary>
    /// Implements customer insights and RFM value segmentations.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICampaignResponseRepository _responseRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        public CustomerService(
            ICustomerRepository customerRepository,
            ICampaignResponseRepository responseRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
        }

        /// <summary>
        /// Gets all customers.
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> GetCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(c => MapToDto(c));
        }

        /// <summary>
        /// Gets a customer by their ID.
        /// </summary>
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return customer == null ? null : MapToDto(customer);
        }

        /// <summary>
        /// Gets full customer insights (including demographic bins and RFM list).
        /// </summary>
        public async Task<CustomerInsightsDto> GetCustomerInsightsAsync()
        {
            var demographics = await GetCustomerDemographicsAsync();
            var rfm = await GetCustomerRfmTiersAsync();

            return new CustomerInsightsDto
            {
                AgeDistribution = demographics.AgeDistribution,
                IncomeDistribution = demographics.IncomeDistribution,
                RfmSegments = rfm
            };
        }

        /// <summary>
        /// Calculates age, gender, income, and country distributions dynamically.
        /// </summary>
        public async Task<CustomerDemographicsDto> GetCustomerDemographicsAsync()
        {
            var customers = (await _customerRepository.GetAllAsync()).ToList();

            // 1. Age Distribution
            var ageDist = new List<DemographicSegmentDto>
            {
                new() { Range = "< 30", Count = customers.Count(c => c.Age < 30) },
                new() { Range = "30 - 45", Count = customers.Count(c => c.Age >= 30 && c.Age <= 45) },
                new() { Range = "46 - 60", Count = customers.Count(c => c.Age >= 46 && c.Age <= 60) },
                new() { Range = "60+", Count = customers.Count(c => c.Age > 60) }
            };

            // 2. Gender Distribution
            var genderDist = customers
                .GroupBy(c => c.Gender)
                .Select(g => new DemographicSegmentDto { Range = g.Key, Count = g.Count() })
                .ToList();

            // 3. Income Distribution
            var incomeDist = new List<DemographicSegmentDto>
            {
                new() { Range = "< $30,000", Count = customers.Count(c => c.Income < 30000) },
                new() { Range = "$30,000 - $60,000", Count = customers.Count(c => c.Income >= 30000 && c.Income <= 60000) },
                new() { Range = "$60,001 - $90,000", Count = customers.Count(c => c.Income >= 60001 && c.Income <= 90000) },
                new() { Range = "$90,000+", Count = customers.Count(c => c.Income > 90000) }
            };

            // 4. Country Distribution
            var countryDist = customers
                .GroupBy(c => c.Country)
                .Select(g => new DemographicSegmentDto { Range = g.Key, Count = g.Count() })
                .ToList();

            return new CustomerDemographicsDto
            {
                AgeDistribution = ageDist,
                GenderDistribution = genderDist,
                IncomeDistribution = incomeDist,
                CountryDistribution = countryDist
            };
        }

        /// <summary>
        /// Segments customers into High, Medium, and Low Value tiers using percentile-based logic on total spend.
        /// </summary>
        public async Task<List<RfmSegmentDto>> GetCustomerRfmTiersAsync()
        {
            var customerSpending = await _responseRepository.Query()
                .GroupBy(r => r.CustomerId)
                .Select(g => new {
                    CustomerId = g.Key,
                    TotalSpend = g.Sum(r => r.PurchaseAmount),
                    TotalPurchases = g.Sum(r => r.NumberOfPurchases)
                })
                .ToListAsync();

            if (!customerSpending.Any())
            {
                return new List<RfmSegmentDto>
                {
                    new() { Segment = "High Value", Count = 0, AverageSpend = 0, Percentage = 0 },
                    new() { Segment = "Medium Value", Count = 0, AverageSpend = 0, Percentage = 0 },
                    new() { Segment = "Low Value", Count = 0, AverageSpend = 0, Percentage = 0 }
                };
            }

            int totalCustomers = customerSpending.Count;
            var sorted = customerSpending.OrderBy(c => c.TotalSpend).ToList();

            int tierSize = totalCustomers / 3;
            if (tierSize == 0) tierSize = 1;

            var lowTier = sorted.Take(tierSize).ToList();
            var medTier = sorted.Skip(tierSize).Take(totalCustomers - 2 * tierSize).ToList();
            var highTier = sorted.Skip(totalCustomers - tierSize).ToList();

            var rfmSegments = new List<RfmSegmentDto>
            {
                new()
                {
                    Segment = "High Value",
                    Count = highTier.Count,
                    AverageSpend = highTier.Any() ? Math.Round(highTier.Average(x => x.TotalSpend), 2) : 0,
                    Percentage = Math.Round((double)highTier.Count / totalCustomers, 4)
                },
                new()
                {
                    Segment = "Medium Value",
                    Count = medTier.Count,
                    AverageSpend = medTier.Any() ? Math.Round(medTier.Average(x => x.TotalSpend), 2) : 0,
                    Percentage = Math.Round((double)medTier.Count / totalCustomers, 4)
                },
                new()
                {
                    Segment = "Low Value",
                    Count = lowTier.Count,
                    AverageSpend = lowTier.Any() ? Math.Round(lowTier.Average(x => x.TotalSpend), 2) : 0,
                    Percentage = Math.Round((double)lowTier.Count / totalCustomers, 4)
                }
            };

            return rfmSegments;
        }

        private static CustomerDto MapToDto(Customer c)
        {
            return new CustomerDto
            {
                CustomerId = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Gender = c.Gender,
                Age = c.Age,
                Income = c.Income,
                Education = c.Education,
                MaritalStatus = c.MaritalStatus,
                Country = c.Country,
                City = c.City,
                CreatedAt = c.CreatedAt
            };
        }
    }
}
