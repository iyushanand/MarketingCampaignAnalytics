using System.Collections.Generic;

namespace Backend.DTOs
{
    public class EdaResultDto
    {
        public Dictionary<string, int> MissingValues { get; set; } = new();
        public int DuplicateCount { get; set; }
        public List<OutlierDetailDto> Outliers { get; set; } = new();
        public Dictionary<string, Dictionary<string, double>> CorrelationHeatmap { get; set; } = new();
        public Dictionary<string, List<double>> Distributions { get; set; } = new();
        public Dictionary<string, SummaryStatsDto> SummaryStatistics { get; set; } = new();
    }

    public class OutlierDetailDto
    {
        public string Column { get; set; } = string.Empty;
        public int OutlierCount { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
    }

    public class SummaryStatsDto
    {
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double StdDev { get; set; }
    }
}
