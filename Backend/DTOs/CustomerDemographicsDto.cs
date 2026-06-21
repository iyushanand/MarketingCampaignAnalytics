using System.Collections.Generic;

namespace Backend.DTOs
{
    public class CustomerDemographicsDto
    {
        public List<DemographicSegmentDto> AgeDistribution { get; set; } = new();
        public List<DemographicSegmentDto> GenderDistribution { get; set; } = new();
        public List<DemographicSegmentDto> IncomeDistribution { get; set; } = new();
        public List<DemographicSegmentDto> CountryDistribution { get; set; } = new();
        public List<DemographicSegmentDto> EducationDistribution { get; set; } = new();

        // Cross-sectional comparisons
        public List<DemographicCompareDto> IncomeVsSpending { get; set; } = new();
        public List<DemographicCompareDto> AgeVsSpending { get; set; } = new();
        public List<DemographicCompareDto> EducationVsResponse { get; set; } = new();
        public List<DemographicCompareDto> CountryVsResponse { get; set; } = new();
    }

    public class DemographicCompareDto
    {
        public string Category { get; set; } = string.Empty; // e.g. "PhD" or "30-45"
        public decimal AverageSpend { get; set; }
        public double ResponseRate { get; set; }
        public int Count { get; set; }
    }
}
