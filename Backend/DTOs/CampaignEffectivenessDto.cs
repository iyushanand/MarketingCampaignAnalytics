namespace Backend.DTOs
{
    public class CampaignEffectivenessDto
    {
        public string CampaignName { get; set; } = string.Empty;
        public decimal Spend { get; set; }
        public decimal Revenue { get; set; }
        public decimal Roi { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal ResponseRate { get; set; }
        public bool IsBestRoi { get; set; }
        public bool IsWorstRoi { get; set; }
        public bool IsBestResponse { get; set; }
    }
}
