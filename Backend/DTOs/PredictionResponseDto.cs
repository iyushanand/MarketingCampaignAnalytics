using System.Collections.Generic;

namespace Backend.DTOs
{
    public class PredictionResponseDto
    {
        public string Prediction { get; set; } = string.Empty; // "Likely Response" or "Not Likely Response"
        public double Probability { get; set; }
        public string ConfidenceLevel { get; set; } = string.Empty; // "High", "Medium", "Low"
        public List<string> BusinessReasons { get; set; } = new();
    }
}
