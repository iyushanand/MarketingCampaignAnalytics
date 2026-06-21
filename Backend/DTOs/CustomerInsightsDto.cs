using System.Collections.Generic;

namespace Backend.DTOs
{
    public class CustomerInsightsDto
    {
        public List<DemographicSegmentDto> AgeDistribution { get; set; } = new();
        public List<DemographicSegmentDto> IncomeDistribution { get; set; } = new();
        public List<RfmSegmentDto> RfmSegments { get; set; } = new();
    }

    public class DemographicSegmentDto
    {
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RfmSegmentDto
    {
        public string Segment { get; set; } = string.Empty; // "High Value", "Medium Value", "Low Value"
        public int Count { get; set; }
        public decimal AverageSpend { get; set; }
        public double Percentage { get; set; }
    }
}
