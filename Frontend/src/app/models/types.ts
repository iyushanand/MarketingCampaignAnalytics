export interface DashboardKpis {
  totalRevenue: number;
  campaignSpend: number;
  roi: number;
  totalCampaigns: number;
  totalCustomers: number;
  averageOrderValue: number;
  bestMarketingChannel: string;
  conversionRate: number;
}

export interface CampaignPerformance {
  campaignName: string;
  spend: number;
  revenue: number;
  roi: number;
  conversionRate: number;
  ctr: number;
}

export interface CampaignComparison {
  campaignName: string;
  spend: number;
  revenue: number;
  roi: number;
  conversionRate: number;
}

export interface CampaignEffectiveness {
  campaignName: string;
  spend: number;
  revenue: number;
  roi: number;
  conversionRate: number;
  responseRate: number;
  isBestRoi: boolean;
  isWorstRoi: boolean;
  isBestResponse: boolean;
}

export interface DemographicSegment {
  range: string;
  count: number;
}

export interface RfmSegment {
  segment: string;
  count: number;
  averageSpend: number;
  percentage: number;
}

export interface CustomerInsights {
  ageDistribution: DemographicSegment[];
  incomeDistribution: DemographicSegment[];
  rfmSegments: RfmSegment[];
}

export interface TTestResult {
  tStatistic: number;
  pValue: number;
  businessExplanation: string;
}

export interface ChiSquareResult {
  chiSquareStatistic: number;
  pValue: number;
  degreesOfFreedom: number;
  businessExplanation: string;
}

export interface CorrelationItem {
  variable1: string;
  variable2: string;
  coefficient: number;
}

export interface RegressionCoefficient {
  feature: string;
  coefficient: number;
  pValue: number;
}

export interface RegressionResult {
  rSquared: number;
  intercept: number;
  coefficients: RegressionCoefficient[];
  businessExplanation: string;
}

export interface StatisticsResult {
  tTest: TTestResult;
  chiSquare: ChiSquareResult;
  correlations: CorrelationItem[];
  regression: RegressionResult;
}

export interface OutlierDetail {
  column: string;
  outlierCount: number;
  lowerBound: number;
  upperBound: number;
}

export interface SummaryStats {
  mean: number;
  median: number;
  min: number;
  max: number;
  stdDev: number;
}

export interface EdaResult {
  missingValues: { [key: string]: number };
  duplicateCount: number;
  outliers: OutlierDetail[];
  correlationHeatmap: { [key: string]: { [key: string]: number } };
  distributions: { [key: string]: number[] };
  summaryStatistics: { [key: string]: SummaryStats };
}

export interface PredictionRequest {
  age: number;
  income: number;
  totalSpend: number;
  totalPurchases: number;
}

export interface PredictionResponse {
  prediction: string;
  probability: number;
}
