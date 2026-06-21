using System;

namespace Backend.DTOs
{
    public class CampaignResponseDto
    {
        public int ResponseId { get; set; }
        public int CustomerId { get; set; }
        public int CampaignId { get; set; }
        public string Response { get; set; } = "No";
        public decimal PurchaseAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int NumberOfPurchases { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
