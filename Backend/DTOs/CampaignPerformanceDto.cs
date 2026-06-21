namespace Backend.DTOs
{
    public class CampaignPerformanceDto
    {
        public string CampaignName { get; set; } = string.Empty;
        public string MarketingChannel { get; set; } = string.Empty;
        public decimal Spend { get; set; }
        public decimal Revenue { get; set; }
        public decimal Roi { get; set; }
        public decimal ConversionRate { get; set; }
        public double Ctr { get; set; }
    }
}
