using System.Collections.Generic;

namespace Backend.DTOs
{
    public class PredictionMetricsDto
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public double RocAuc { get; set; }
        public List<List<int>> ConfusionMatrix { get; set; } = new();
        public string ClassificationReport { get; set; } = string.Empty;
    }
}
