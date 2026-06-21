using System.Collections.Generic;

namespace Backend.DTOs
{
    public class StatisticsResultDto
    {
        public TTestResultDto TTest { get; set; } = new();
        public ChiSquareResultDto ChiSquare { get; set; } = new();
        public List<CorrelationItemDto> Correlations { get; set; } = new();
        public RegressionResultDto Regression { get; set; } = new();
    }

    public class TTestResultDto
    {
        public double TStatistic { get; set; }
        public double PValue { get; set; }
        public string BusinessExplanation { get; set; } = string.Empty;
    }

    public class ChiSquareResultDto
    {
        public double ChiSquareStatistic { get; set; }
        public double PValue { get; set; }
        public int DegreesOfFreedom { get; set; }
        public string BusinessExplanation { get; set; } = string.Empty;
    }

    public class CorrelationItemDto
    {
        public string Variable1 { get; set; } = string.Empty;
        public string Variable2 { get; set; } = string.Empty;
        public double Coefficient { get; set; }
    }

    public class RegressionResultDto
    {
        public double RSquared { get; set; }
        public double Intercept { get; set; }
        public List<RegressionCoefficientDto> Coefficients { get; set; } = new();
        public string BusinessExplanation { get; set; } = string.Empty;
    }

    public class RegressionCoefficientDto
    {
        public string Feature { get; set; } = string.Empty;
        public double Coefficient { get; set; }
        public double PValue { get; set; }
    }
}
