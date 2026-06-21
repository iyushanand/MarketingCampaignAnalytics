namespace Backend.DTOs
{
    public class CustomerPersonaDto
    {
        public string PersonaName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public decimal AverageIncome { get; set; }
        public decimal AverageSpending { get; set; }
        public double AveragePurchases { get; set; }
        public double AverageResponseRate { get; set; }
    }
}
