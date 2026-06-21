import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  DashboardKpis, 
  CampaignPerformance, 
  CampaignComparison, 
  CampaignEffectiveness,
  CustomerInsights, 
  CustomerDemographics, // we will add it
  RfmSegment,
  EdaResult, 
  StatisticsResult, 
  PredictionRequest, 
  PredictionResponse,
  CampaignDto,
  CustomerDto
} from '../models/types';

// Let's add ApiResponse definition directly to make it self-contained
export interface ApiResponseWrapper<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5224/api'; // Correct ASP.NET Core port from Phase 3 logs

  constructor(private http: HttpClient) { }

  getDashboardKpis(): Observable<ApiResponseWrapper<DashboardKpis>> {
    return this.http.get<ApiResponseWrapper<DashboardKpis>>(`${this.apiUrl}/dashboard/kpis`);
  }

  getRevenueTrend(): Observable<ApiResponseWrapper<any[]>> {
    return this.http.get<ApiResponseWrapper<any[]>>(`${this.apiUrl}/dashboard/revenue-trend`);
  }

  getTopCampaigns(): Observable<ApiResponseWrapper<CampaignDto[]>> {
    return this.http.get<ApiResponseWrapper<CampaignDto[]>>(`${this.apiUrl}/dashboard/top-campaigns`);
  }

  getCampaigns(): Observable<ApiResponseWrapper<CampaignDto[]>> {
    return this.http.get<ApiResponseWrapper<CampaignDto[]>>(`${this.apiUrl}/campaign`);
  }

  getCampaignById(id: number): Observable<ApiResponseWrapper<CampaignDto>> {
    return this.http.get<ApiResponseWrapper<CampaignDto>>(`${this.apiUrl}/campaign/${id}`);
  }

  getCampaignPerformance(): Observable<ApiResponseWrapper<CampaignPerformance[]>> {
    return this.http.get<ApiResponseWrapper<CampaignPerformance[]>>(`${this.apiUrl}/campaign/performance`);
  }

  getCampaignComparison(): Observable<ApiResponseWrapper<CampaignComparison[]>> {
    return this.http.get<ApiResponseWrapper<CampaignComparison[]>>(`${this.apiUrl}/campaign/comparison`);
  }

  getCampaignEffectiveness(): Observable<ApiResponseWrapper<CampaignEffectiveness[]>> {
    return this.http.get<ApiResponseWrapper<CampaignEffectiveness[]>>(`${this.apiUrl}/campaign/effectiveness`);
  }

  getCustomers(): Observable<ApiResponseWrapper<CustomerDto[]>> {
    return this.http.get<ApiResponseWrapper<CustomerDto[]>>(`${this.apiUrl}/customer`);
  }

  getCustomerById(id: number): Observable<ApiResponseWrapper<CustomerDto>> {
    return this.http.get<ApiResponseWrapper<CustomerDto>>(`${this.apiUrl}/customer/${id}`);
  }

  getCustomerInsights(): Observable<ApiResponseWrapper<CustomerInsights>> {
    return this.http.get<ApiResponseWrapper<CustomerInsights>>(`${this.apiUrl}/customer/insights`);
  }

  getCustomerDemographics(): Observable<ApiResponseWrapper<CustomerDemographics>> {
    return this.http.get<ApiResponseWrapper<CustomerDemographics>>(`${this.apiUrl}/customer/demographics`);
  }

  getCustomerRfmTiers(): Observable<ApiResponseWrapper<RfmSegment[]>> {
    return this.http.get<ApiResponseWrapper<RfmSegment[]>>(`${this.apiUrl}/customer/rfm`);
  }

  loadSampleDataset(): Observable<ApiResponseWrapper<string>> {
    return this.http.post<ApiResponseWrapper<string>>(`${this.apiUrl}/upload/sample`, {});
  }

  uploadCsv(file: File): Observable<ApiResponseWrapper<any>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponseWrapper<any>>(`${this.apiUrl}/upload/csv`, formData);
  }

  getMarketingReportData(reportType: string): Observable<any[]> {
    // Note: Reports controller returns raw JSON content content(json, "application/json")
    return this.http.get<any[]>(`${this.apiUrl}/reports/${reportType}`);
  }

  downloadExcelReport(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/report/download/excel`, { responseType: 'blob' });
  }

  downloadPdfReport(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/report/download/pdf`, { responseType: 'blob' });
  }
}
