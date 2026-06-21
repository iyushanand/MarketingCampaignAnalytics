namespace Backend.DTOs
{
    public class CampaignComparisonDto
    {
        public string CampaignName { get; set; } = string.Empty;
        public decimal Spend { get; set; }
        public decimal Revenue { get; set; }
        public decimal Roi { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
