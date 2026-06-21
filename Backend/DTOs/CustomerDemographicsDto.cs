using System.Collections.Generic;

namespace Backend.DTOs
{
    public class CustomerDemographicsDto
    {
        public List<DemographicSegmentDto> AgeDistribution { get; set; } = new();
        public List<DemographicSegmentDto> GenderDistribution { get; set; } = new();
        public List<DemographicSegmentDto> IncomeDistribution { get; set; } = new();
        public List<DemographicSegmentDto> CountryDistribution { get; set; } = new();
    }
}
