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
    /// Implements simplified customer analytics and RFM value segmentations.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICampaignResponseRepository _responseRepository;
        private readonly ICampaignRepository _campaignRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        public CustomerService(
            ICustomerRepository customerRepository,
            ICampaignResponseRepository responseRepository,
            ICampaignRepository campaignRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
        }

        /// <summary>
        /// Gets all customers with details, RFM metrics, and segment tags.
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> GetCustomersAsync()
        {
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var responses = (await _responseRepository.GetAllAsync()).ToList();
            
            var customerStats = GetCustomerRfmMap(responses);
            decimal avgSpend = customers.Any() ? responses.Where(r => r.Response == "Yes").Sum(r => r.PurchaseAmount) / customers.Count : 0;
            decimal highThreshold = avgSpend * 1.2m;
            decimal lowThreshold = avgSpend * 0.5m;

            return customers.Select(c =>
            {
                var stats = customerStats.GetValueOrDefault(c.CustomerId);
                int recency = stats?.Recency ?? 180;
                int frequency = stats?.Frequency ?? 0;
                decimal monetary = stats?.Monetary ?? 0m;
                double responseRate = stats?.ResponseCount > 0 ? (double)stats.YesCount / stats.ResponseCount : 0.0;
                
                string segment = "Medium Value";
                if (monetary >= highThreshold) segment = "High Value";
                else if (monetary < lowThreshold) segment = "Low Value";

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
                    CreatedAt = c.CreatedAt,
                    Recency = recency,
                    Frequency = frequency,
                    Monetary = monetary,
                    RfmSegment = segment,
                    ResponseRate = Math.Round(responseRate, 4)
                };
            });
        }

        /// <summary>
        /// Gets a customer by their ID.
        /// </summary>
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return null;

            var responses = await _responseRepository.FindAsync(r => r.CustomerId == id);
            var customerPurchases = responses.Where(r => r.Response == "Yes").ToList();
            
            DateTime baselineDate = new DateTime(2026, 7, 1);
            int recency = customerPurchases.Any() ? (baselineDate - customerPurchases.Max(x => x.PurchaseDate)).Days : 180;
            int frequency = customerPurchases.Sum(r => r.NumberOfPurchases);
            decimal monetary = customerPurchases.Sum(r => r.PurchaseAmount);
            double responseRate = responses.Any() ? (double)responses.Count(r => r.Response == "Yes") / responses.Count() : 0.0;

            // Use hardcoded general threshold as fallback for single customer detail
            string segment = "Medium Value";
            if (monetary >= 500) segment = "High Value";
            else if (monetary < 150) segment = "Low Value";

            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Gender = customer.Gender,
                Age = customer.Age,
                Income = customer.Income,
                Education = customer.Education,
                MaritalStatus = customer.MaritalStatus,
                Country = customer.Country,
                City = customer.City,
                CreatedAt = customer.CreatedAt,
                Recency = recency,
                Frequency = frequency,
                Monetary = monetary,
                RfmSegment = segment,
                ResponseRate = Math.Round(responseRate, 4)
            };
        }

        /// <summary>
        /// Gets consolidated KPIs and behavior metrics.
        /// </summary>
        public async Task<CustomerAnalyticsDto> GetCustomerSummaryAsync()
        {
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var responses = (await _responseRepository.GetAllAsync()).ToList();

            int totalCustomers = customers.Count;
            if (totalCustomers == 0) return new CustomerAnalyticsDto();

            var customerStats = GetCustomerRfmMap(responses);
            decimal totalSpendAll = responses.Where(r => r.Response == "Yes").Sum(r => r.PurchaseAmount);
            int totalPurchasesAll = responses.Where(r => r.Response == "Yes").Sum(r => r.NumberOfPurchases);
            double avgAge = customers.Average(c => c.Age);
            decimal avgIncome = customers.Average(c => c.Income);
            decimal avgCustomerSpend = totalSpendAll / totalCustomers;
            double avgPurchases = (double)totalPurchasesAll / totalCustomers;
            double overallResponseRate = responses.Any() ? (double)responses.Count(r => r.Response == "Yes") / responses.Count : 0.0;

            // Simple RFM Segmentation
            decimal highThreshold = avgCustomerSpend * 1.2m;
            decimal lowThreshold = avgCustomerSpend * 0.5m;

            var customerDetailed = customers.Select(c =>
            {
                var stats = customerStats.GetValueOrDefault(c.CustomerId);
                decimal monetary = stats?.Monetary ?? 0m;
                int frequency = stats?.Frequency ?? 0;
                return new { Customer = c, Monetary = monetary, Frequency = frequency };
            }).ToList();

            var highTier = customerDetailed.Where(x => x.Monetary >= highThreshold).ToList();
            var medTier = customerDetailed.Where(x => x.Monetary >= lowThreshold && x.Monetary < highThreshold).ToList();
            var lowTier = customerDetailed.Where(x => x.Monetary < lowThreshold).ToList();

            // Customer Behaviour Metrics
            decimal avgPurchaseAmt = totalPurchasesAll > 0 ? totalSpendAll / totalPurchasesAll : 0m;
            int repeatPurchasesCount = customerDetailed.Count(x => x.Frequency > 1);
            double repeatPurchaseRate = (double)repeatPurchasesCount / totalCustomers;

            var topSpenders = customerDetailed
                .OrderByDescending(x => x.Monetary)
                .Take(10)
                .Select(x => new CustomerSpendSummaryDto
                {
                    CustomerId = x.Customer.CustomerId,
                    FullName = $"{x.Customer.FirstName} {x.Customer.LastName}",
                    Country = x.Customer.Country,
                    TotalSpend = x.Monetary,
                    TotalPurchases = x.Frequency
                })
                .ToList();

            var mostActive = customerDetailed
                .OrderByDescending(x => x.Frequency)
                .Take(10)
                .Select(x => new CustomerSpendSummaryDto
                {
                    CustomerId = x.Customer.CustomerId,
                    FullName = $"{x.Customer.FirstName} {x.Customer.LastName}",
                    Country = x.Customer.Country,
                    TotalSpend = x.Monetary,
                    TotalPurchases = x.Frequency
                })
                .ToList();

            return new CustomerAnalyticsDto
            {
                TotalCustomers = totalCustomers,
                AverageIncome = Math.Round(avgIncome, 2),
                AverageCustomerSpend = Math.Round(avgCustomerSpend, 2),
                AveragePurchases = Math.Round(avgPurchases, 2),
                AverageResponseRate = Math.Round(overallResponseRate, 4),
                HighValueCustomers = new RfmSegmentSummaryDto
                {
                    Count = highTier.Count,
                    Percentage = Math.Round((double)highTier.Count / totalCustomers, 4),
                    AverageSpend = highTier.Any() ? Math.Round(highTier.Average(x => x.Monetary), 2) : 0m,
                    AveragePurchases = highTier.Any() ? Math.Round(highTier.Average(x => x.Frequency), 2) : 0.0,
                    RevenueContribution = highTier.Sum(x => x.Monetary)
                },
                MediumValueCustomers = new RfmSegmentSummaryDto
                {
                    Count = medTier.Count,
                    Percentage = Math.Round((double)medTier.Count / totalCustomers, 4),
                    AverageSpend = medTier.Any() ? Math.Round(medTier.Average(x => x.Monetary), 2) : 0m,
                    AveragePurchases = medTier.Any() ? Math.Round(medTier.Average(x => x.Frequency), 2) : 0.0,
                    RevenueContribution = medTier.Sum(x => x.Monetary)
                },
                LowValueCustomers = new RfmSegmentSummaryDto
                {
                    Count = lowTier.Count,
                    Percentage = Math.Round((double)lowTier.Count / totalCustomers, 4),
                    AverageSpend = lowTier.Any() ? Math.Round(lowTier.Average(x => x.Monetary), 2) : 0m,
                    AveragePurchases = lowTier.Any() ? Math.Round(lowTier.Average(x => x.Frequency), 2) : 0.0,
                    RevenueContribution = lowTier.Sum(x => x.Monetary)
                },
                AveragePurchaseAmount = Math.Round(avgPurchaseAmt, 2),
                AverageCustomerLifetimeSpend = Math.Round(avgCustomerSpend, 2),
                RepeatPurchaseRate = Math.Round(repeatPurchaseRate, 4),
                TopSpendingCustomers = topSpenders,
                MostActiveCustomers = mostActive
            };
        }

        /// <summary>
        /// Gets rule-based customer personas.
        /// </summary>
        public async Task<IEnumerable<CustomerPersonaDto>> GetCustomerPersonasAsync()
        {
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var responses = (await _responseRepository.GetAllAsync()).ToList();

            int totalCustomers = customers.Count;
            if (totalCustomers == 0) return Enumerable.Empty<CustomerPersonaDto>();

            var customerStats = GetCustomerRfmMap(responses);
            decimal avgSpend = customers.Any() ? responses.Where(r => r.Response == "Yes").Sum(r => r.PurchaseAmount) / customers.Count : 0;
            decimal highThreshold = avgSpend * 1.2m;
            decimal lowThreshold = avgSpend * 0.5m;

            var categorized = customers.Select(c =>
            {
                var stats = customerStats.GetValueOrDefault(c.CustomerId);
                int recency = stats?.Recency ?? 180;
                int frequency = stats?.Frequency ?? 0;
                decimal monetary = stats?.Monetary ?? 0m;
                double responseRate = stats?.ResponseCount > 0 ? (double)stats.YesCount / stats.ResponseCount : 0.0;
                
                string segment = "Medium Value";
                if (monetary >= highThreshold) segment = "High Value";
                else if (monetary < lowThreshold) segment = "Low Value";

                string persona;
                string description;
                if ((segment == "High Value" || segment == "Medium Value") && recency >= 90)
                {
                    persona = "At Risk Customers";
                    description = "High or medium spending customers with no recent purchase in the last 90+ days.";
                }
                else if (segment == "High Value")
                {
                    persona = "High Value Customers";
                    description = "Premium tier spenders with active and recent transaction profiles.";
                }
                else if (frequency >= 10)
                {
                    persona = "Frequent Buyers";
                    description = "High frequency shoppers with consistent brand touchpoints.";
                }
                else
                {
                    persona = "Occasional Buyers";
                    description = "Transactional customers with low purchase counts and sporadic visits.";
                }

                return new {
                    Customer = c,
                    Persona = persona,
                    Description = description,
                    Monetary = monetary,
                    Frequency = frequency,
                    ResponseRate = responseRate
                };
            }).ToList();

            var personasList = new List<CustomerPersonaDto>();
            var personaNames = new[] { "High Value Customers", "Frequent Buyers", "Occasional Buyers", "At Risk Customers" };

            foreach (var pName in personaNames)
            {
                var group = categorized.Where(x => x.Persona == pName).ToList();
                personasList.Add(new CustomerPersonaDto
                {
                    PersonaName = pName,
                    Description = group.FirstOrDefault()?.Description ?? "Customer profile persona segment.",
                    CustomerCount = group.Count,
                    AverageIncome = group.Any() ? Math.Round(group.Average(x => x.Customer.Income), 2) : 0m,
                    AverageSpending = group.Any() ? Math.Round(group.Average(x => x.Monetary), 2) : 0m,
                    AveragePurchases = group.Any() ? Math.Round(group.Average(x => x.Frequency), 2) : 0.0,
                    AverageResponseRate = group.Any() ? Math.Round(group.Average(x => x.ResponseRate), 4) : 0.0
                });
            }

            return personasList;
        }

        /// <summary>
        /// Gets demographic distributions and cross-sectional analysis.
        /// </summary>
        public async Task<CustomerDemographicsDto> GetCustomerAnalyticsAsync()
        {
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var responses = (await _responseRepository.GetAllAsync()).ToList();
            var customerStats = GetCustomerRfmMap(responses);

            var detailed = customers.Select(c =>
            {
                var stats = customerStats.GetValueOrDefault(c.CustomerId);
                decimal monetary = stats?.Monetary ?? 0m;
                int yesCount = stats?.YesCount ?? 0;
                int responseCount = stats?.ResponseCount ?? 0;
                return new { Customer = c, Monetary = monetary, YesCount = yesCount, ResponseCount = responseCount };
            }).ToList();

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

            // 3. Education Distribution
            var eduDist = customers
                .GroupBy(c => c.Education)
                .Select(g => new DemographicSegmentDto { Range = g.Key, Count = g.Count() })
                .ToList();

            // 4. Country Distribution
            var countryDist = customers
                .GroupBy(c => c.Country)
                .Select(g => new DemographicSegmentDto { Range = g.Key, Count = g.Count() })
                .ToList();

            // 5. Income Distribution
            var incomeDist = new List<DemographicSegmentDto>
            {
                new() { Range = "< $30,000", Count = customers.Count(c => c.Income < 30000) },
                new() { Range = "$30,000 - $60,000", Count = customers.Count(c => c.Income >= 30000 && c.Income <= 60000) },
                new() { Range = "$60,001 - $90,000", Count = customers.Count(c => c.Income >= 60001 && c.Income <= 90000) },
                new() { Range = "$90,000+", Count = customers.Count(c => c.Income > 90000) }
            };

            // 6. Income vs Spending Comparison
            var incomeBins = new[]
            {
                new { Range = "< $30,000", Predicate = new Func<Customer, bool>(c => c.Income < 30000) },
                new { Range = "$30,000 - $60,000", Predicate = new Func<Customer, bool>(c => c.Income >= 30000 && c.Income <= 60000) },
                new { Range = "$60,001 - $90,000", Predicate = new Func<Customer, bool>(c => c.Income >= 60001 && c.Income <= 90000) },
                new { Range = "$90,000+", Predicate = new Func<Customer, bool>(c => c.Income > 90000) }
            };
            var incomeCompare = incomeBins.Select(bin =>
            {
                var matches = detailed.Where(x => bin.Predicate(x.Customer)).ToList();
                return new DemographicCompareDto
                {
                    Category = bin.Range,
                    AverageSpend = matches.Any() ? Math.Round(matches.Average(x => x.Monetary), 2) : 0m,
                    ResponseRate = matches.Sum(x => x.ResponseCount) > 0 ? (double)matches.Sum(x => x.YesCount) / matches.Sum(x => x.ResponseCount) : 0.0,
                    Count = matches.Count
                };
            }).ToList();

            // 7. Age vs Spending Comparison
            var ageBins = new[]
            {
                new { Range = "< 30", Predicate = new Func<Customer, bool>(c => c.Age < 30) },
                new { Range = "30 - 45", Predicate = new Func<Customer, bool>(c => c.Age >= 30 && c.Age <= 45) },
                new { Range = "46 - 60", Predicate = new Func<Customer, bool>(c => c.Age >= 46 && c.Age <= 60) },
                new { Range = "60+", Predicate = new Func<Customer, bool>(c => c.Age > 60) }
            };
            var ageCompare = ageBins.Select(bin =>
            {
                var matches = detailed.Where(x => bin.Predicate(x.Customer)).ToList();
                return new DemographicCompareDto
                {
                    Category = bin.Range,
                    AverageSpend = matches.Any() ? Math.Round(matches.Average(x => x.Monetary), 2) : 0m,
                    ResponseRate = matches.Sum(x => x.ResponseCount) > 0 ? (double)matches.Sum(x => x.YesCount) / matches.Sum(x => x.ResponseCount) : 0.0,
                    Count = matches.Count
                };
            }).ToList();

            // 8. Education vs Response Comparison
            var eduCompare = detailed
                .GroupBy(x => x.Customer.Education)
                .Select(g => new DemographicCompareDto
                {
                    Category = g.Key,
                    AverageSpend = g.Any() ? Math.Round(g.Average(x => x.Monetary), 2) : 0m,
                    ResponseRate = g.Sum(x => x.ResponseCount) > 0 ? Math.Round((double)g.Sum(x => x.YesCount) / g.Sum(x => x.ResponseCount), 4) : 0.0,
                    Count = g.Count()
                })
                .ToList();

            // 9. Country vs Response Comparison
            var countryCompare = detailed
                .GroupBy(x => x.Customer.Country)
                .Select(g => new DemographicCompareDto
                {
                    Category = g.Key,
                    AverageSpend = g.Any() ? Math.Round(g.Average(x => x.Monetary), 2) : 0m,
                    ResponseRate = g.Sum(x => x.ResponseCount) > 0 ? Math.Round((double)g.Sum(x => x.YesCount) / g.Sum(x => x.ResponseCount), 4) : 0.0,
                    Count = g.Count()
                })
                .ToList();

            return new CustomerDemographicsDto
            {
                AgeDistribution = ageDist,
                GenderDistribution = genderDist,
                EducationDistribution = eduDist,
                CountryDistribution = countryDist,
                IncomeDistribution = incomeDist,
                IncomeVsSpending = incomeCompare,
                AgeVsSpending = ageCompare,
                EducationVsResponse = eduCompare,
                CountryVsResponse = countryCompare
            };
        }

        private static Dictionary<int, CustomerRfmAccumulator> GetCustomerRfmMap(List<CampaignResponse> responses)
        {
            DateTime baselineDate = new DateTime(2026, 7, 1);
            var map = new Dictionary<int, CustomerRfmAccumulator>();
            
            foreach (var r in responses)
            {
                if (!map.TryGetValue(r.CustomerId, out var acc))
                {
                    acc = new CustomerRfmAccumulator { CustomerId = r.CustomerId };
                    map[r.CustomerId] = acc;
                }

                acc.ResponseCount++;
                if (r.Response == "Yes")
                {
                    acc.YesCount++;
                    acc.Frequency += r.NumberOfPurchases;
                    acc.Monetary += r.PurchaseAmount;
                    
                    int days = (baselineDate - r.PurchaseDate).Days;
                    if (days < acc.Recency)
                    {
                        acc.Recency = days;
                    }
                }
            }

            return map;
        }

        private class CustomerRfmAccumulator
        {
            public int CustomerId { get; set; }
            public int Recency { get; set; } = 180; // default max days
            public int Frequency { get; set; }
            public decimal Monetary { get; set; }
            public int ResponseCount { get; set; }
            public int YesCount { get; set; }
        }
    }
}
