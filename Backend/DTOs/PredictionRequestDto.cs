namespace Backend.DTOs
{
    public class PredictionRequestDto
    {
        public int Age { get; set; }
        public decimal Income { get; set; }
        public decimal TotalSpend { get; set; }
        public int TotalPurchases { get; set; }
    }
}
