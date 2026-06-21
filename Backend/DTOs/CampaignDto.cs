using System;

namespace Backend.DTOs
{
    public class CampaignDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string CampaignType { get; set; } = string.Empty;
        public string MarketingChannel { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal Spend { get; set; }
        public decimal Revenue { get; set; }
        public int Conversions { get; set; }
        public int Clicks { get; set; }
        public int Impressions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }
}
