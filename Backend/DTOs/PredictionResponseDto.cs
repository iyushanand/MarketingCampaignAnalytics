namespace Backend.DTOs
{
    public class PredictionResponseDto
    {
        public string Prediction { get; set; } = string.Empty; // "Likely Response" or "Not Likely Response"
        public double Probability { get; set; }
    }
}
