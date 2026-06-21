namespace Backend.DTOs
{
    public class PredictionRequestDto
    {
        public int Age { get; set; }
        public decimal Income { get; set; }
        public string Education { get; set; } = string.Empty;
        public int TotalPurchases { get; set; }
        public decimal AverageSpend { get; set; }
        public string CampaignChannel { get; set; } = string.Empty;
    }
}
