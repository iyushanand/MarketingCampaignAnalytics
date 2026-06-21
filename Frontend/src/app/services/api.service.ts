import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  DashboardKpis, 
  CampaignPerformance, 
  CampaignComparison, 
  CampaignEffectiveness,
  CustomerInsights, 
  EdaResult, 
  StatisticsResult, 
  PredictionRequest, 
  PredictionResponse 
} from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5246/api'; // ASP.NET Core URL (adjust as needed or read config)

  constructor(private http: HttpClient) { }

  getDashboardKpis(): Observable<DashboardKpis> {
    return this.http.get<DashboardKpis>(`${this.apiUrl}/dashboard/kpis`);
  }

  getCampaignPerformance(): Observable<CampaignPerformance[]> {
    return this.http.get<CampaignPerformance[]>(`${this.apiUrl}/campaign/performance`);
  }

  getCampaignComparison(): Observable<CampaignComparison[]> {
    return this.http.get<CampaignComparison[]>(`${this.apiUrl}/campaign/comparison`);
  }

  getCampaignEffectiveness(): Observable<CampaignEffectiveness[]> {
    return this.http.get<CampaignEffectiveness[]>(`${this.apiUrl}/campaign/effectiveness`);
  }

  getCustomerInsights(): Observable<CustomerInsights> {
    return this.http.get<CustomerInsights>(`${this.apiUrl}/customer/insights`);
  }

  getEdaResults(): Observable<EdaResult> {
    return this.http.get<EdaResult>(`${this.apiUrl}/analytics/eda`);
  }

  getStatisticsResults(): Observable<StatisticsResult> {
    return this.http.get<StatisticsResult>(`${this.apiUrl}/analytics/statistics`);
  }

  predictResponse(request: PredictionRequest): Observable<PredictionResponse> {
    return this.http.post<PredictionResponse>(`${this.apiUrl}/analytics/predict`, request);
  }

  loadSampleDataset(): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/upload/sample`, {});
  }

  uploadCsv(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/upload/csv`, formData);
  }

  getMarketingReportData(reportType: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/report/marketing-report/${reportType}`);
  }

  downloadExcelReport(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/report/download/excel`, { responseType: 'blob' });
  }

  downloadPdfReport(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/report/download/pdf`, { responseType: 'blob' });
  }
}
