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
  marketingChannel: string;
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
  education: string;
  totalPurchases: number;
  averageSpend: number;
  campaignChannel: string;
}

export interface PredictionResponse {
  prediction: string;
  probability: number;
  confidenceLevel: string;
  businessReasons: string[];
}

export interface PredictionMetrics {
  accuracy: number;
  precision: number;
  recall: number;
  f1Score: number;
  rocAuc: number;
  confusionMatrix: number[][];
  classificationReport: string;
}

export interface DemographicCompare {
  category: string;
  averageSpend: number;
  responseRate: number;
  count: number;
}

export interface CustomerDemographics {
  ageDistribution: DemographicSegment[];
  genderDistribution: DemographicSegment[];
  educationDistribution: DemographicSegment[];
  countryDistribution: DemographicSegment[];
  incomeDistribution: DemographicSegment[];
  incomeVsSpending: DemographicCompare[];
  ageVsSpending: DemographicCompare[];
  educationVsResponse: DemographicCompare[];
  countryVsResponse: DemographicCompare[];
}

export interface RfmSegmentSummary {
  count: number;
  percentage: number;
  averageSpend: number;
  averagePurchases: number;
  revenueContribution: number;
}

export interface CustomerSpendSummary {
  customerId: number;
  fullName: string;
  country: string;
  totalSpend: number;
  totalPurchases: number;
}

export interface CustomerAnalytics {
  totalCustomers: number;
  averageIncome: number;
  averageCustomerSpend: number;
  averagePurchases: number;
  averageResponseRate: number;
  highValueCustomers: RfmSegmentSummary;
  mediumValueCustomers: RfmSegmentSummary;
  lowValueCustomers: RfmSegmentSummary;
  averagePurchaseAmount: number;
  averageCustomerLifetimeSpend: number;
  repeatPurchaseRate: number;
  topSpendingCustomers: CustomerSpendSummary[];
  mostActiveCustomers: CustomerSpendSummary[];
}

export interface CustomerPersona {
  personaName: string;
  description: string;
  customerCount: number;
  averageIncome: number;
  averageSpending: number;
  averagePurchases: number;
  averageResponseRate: number;
}

export interface CustomerDto {
  customerId: number;
  firstName: string;
  lastName: string;
  gender: string;
  age: number;
  income: number;
  education: string;
  maritalStatus: string;
  country: string;
  city: string;
  createdAt: string;
  
  // RFM Metrics & Segmentation
  recency: number;
  frequency: number;
  monetary: number;
  rfmSegment: string;
  responseRate: number;
}

export interface CampaignDto {
  campaignId: number;
  campaignName: string;
  campaignType: string;
  marketingChannel: string;
  budget: number;
  spend: number;
  revenue: number;
  conversions: number;
  clicks: number;
  impressions: number;
  startDate: string;
  endDate: string;
  status: string;
  createdAt: string;
}

export interface ReportFileDto {
  fileName: string;
  fileType: string;
  fileSize: string;
  createdAt: string;
  downloadUrl: string;
}
