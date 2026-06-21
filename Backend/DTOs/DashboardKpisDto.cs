namespace Backend.DTOs
{
    public class DashboardKpisDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal CampaignSpend { get; set; }
        public decimal Roi { get; set; }
        public int TotalCampaigns { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string BestMarketingChannel { get; set; } = string.Empty;
        public decimal ConversionRate { get; set; }
    }
}
