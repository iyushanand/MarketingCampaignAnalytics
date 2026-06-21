import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { CampaignPerformanceComponent } from './components/campaign-performance/campaign-performance.component';
import { CampaignComparisonComponent } from './components/campaign-comparison/campaign-comparison.component';
import { CampaignEffectivenessComponent } from './components/campaign-effectiveness/campaign-effectiveness.component';
import { CustomerInsightsComponent } from './components/customer-insights/customer-insights.component';
import { DataAnalysisComponent } from './components/data-analysis/data-analysis.component';
import { MarketingReportsComponent } from './components/marketing-reports/marketing-reports.component';
import { StatisticsComponent } from './components/statistics/statistics.component';
import { ResponsePredictionComponent } from './components/response-prediction/response-prediction.component';
import { RoiCalculatorComponent } from './components/roi-calculator/roi-calculator.component';
import { ReportsComponent } from './components/reports/reports.component';
import { UploadDatasetComponent } from './components/upload-dataset/upload-dataset.component';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'campaign-performance', component: CampaignPerformanceComponent },
  { path: 'campaign-comparison', component: CampaignComparisonComponent },
  { path: 'campaign-effectiveness', component: CampaignEffectivenessComponent },
  { path: 'customer-insights', component: CustomerInsightsComponent },
  { path: 'data-analysis', component: DataAnalysisComponent },
  { path: 'marketing-reports', component: MarketingReportsComponent },
  { path: 'statistics', component: StatisticsComponent },
  { path: 'response-prediction', component: ResponsePredictionComponent },
  { path: 'roi-calculator', component: RoiCalculatorComponent },
  { path: 'reports', component: ReportsComponent },
  { path: 'upload-dataset', component: UploadDatasetComponent },
  { path: '**', redirectTo: 'dashboard' }
];
