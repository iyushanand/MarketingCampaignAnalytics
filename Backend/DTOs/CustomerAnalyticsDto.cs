using System.Collections.Generic;

namespace Backend.DTOs
{
    public class CustomerAnalyticsDto
    {
        // Customer KPIs
        public int TotalCustomers { get; set; }
        public decimal AverageIncome { get; set; }
        public decimal AverageCustomerSpend { get; set; } // Average lifetime spend per customer
        public double AveragePurchases { get; set; } // Average purchases per customer
        public double AverageResponseRate { get; set; } // Average response rate across all campaigns
        
        // RFM Customer Segments (Counts and Metrics)
        public RfmSegmentSummaryDto HighValueCustomers { get; set; } = new();
        public RfmSegmentSummaryDto MediumValueCustomers { get; set; } = new();
        public RfmSegmentSummaryDto LowValueCustomers { get; set; } = new();

        // Customer Behaviour Metrics
        public decimal AveragePurchaseAmount { get; set; } // Average spend per individual purchase
        public decimal AverageCustomerLifetimeSpend { get; set; } // Average total spend per customer
        public double RepeatPurchaseRate { get; set; } // Percentage of customers with total purchases > 1
        public List<CustomerSpendSummaryDto> TopSpendingCustomers { get; set; } = new();
        public List<CustomerSpendSummaryDto> MostActiveCustomers { get; set; } = new();
    }

    public class RfmSegmentSummaryDto
    {
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageSpend { get; set; }
        public double AveragePurchases { get; set; }
        public decimal RevenueContribution { get; set; }
    }

    public class CustomerSpendSummaryDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal TotalSpend { get; set; }
        public int TotalPurchases { get; set; }
    }
}
