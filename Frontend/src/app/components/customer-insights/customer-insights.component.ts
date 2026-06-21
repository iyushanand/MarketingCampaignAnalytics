import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { RfmSegment, CustomerDemographics } from '../../models/types';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-customer-insights',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-insights.component.html',
  styleUrls: ['./customer-insights.component.css']
})
export class CustomerInsightsComponent implements OnInit, AfterViewInit {
  demographics: CustomerDemographics | null = null;
  rfmSegments: RfmSegment[] = [];
  
  loading = true;
  isDbEmpty = false;

  @ViewChild('ageChartCanvas') ageChartCanvas!: ElementRef;
  @ViewChild('genderChartCanvas') genderChartCanvas!: ElementRef;
  @ViewChild('incomeChartCanvas') incomeChartCanvas!: ElementRef;
  @ViewChild('countryChartCanvas') countryChartCanvas!: ElementRef;

  private ageChart: Chart | null = null;
  private genderChart: Chart | null = null;
  private incomeChart: Chart | null = null;
  private countryChart: Chart | null = null;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadCustomerData();
  }

  ngAfterViewInit() {
    // Dynamic chart instantiation
  }

  loadCustomerData() {
    this.loading = true;
    this.apiService.getCustomerDemographics().subscribe({
      next: (demoResponse) => {
        if (demoResponse.success && demoResponse.data) {
          this.demographics = demoResponse.data;
          this.isDbEmpty = this.demographics.ageDistribution.every(a => a.count === 0);

          if (!this.isDbEmpty) {
            this.loadRfmData();
          } else {
            this.loading = false;
          }
        } else {
          this.isDbEmpty = true;
          this.loading = false;
        }
      },
      error: () => {
        this.isDbEmpty = true;
        this.loading = false;
      }
    });
  }

  loadRfmData() {
    this.apiService.getCustomerRfmTiers().subscribe({
      next: (rfmResponse) => {
        this.loading = false;
        if (rfmResponse.success && rfmResponse.data) {
          this.rfmSegments = rfmResponse.data;
          
          // Trigger chart plotting on next tick once canvases render
          setTimeout(() => {
            this.plotCharts();
          }, 0);
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  plotCharts() {
    if (!this.demographics) return;

    // 1. Age Distribution (Bar)
    if (this.ageChartCanvas) {
      const ageLabels = this.demographics.ageDistribution.map(a => a.range);
      const ageCounts = this.demographics.ageDistribution.map(a => a.count);
      this.ageChart = new Chart(this.ageChartCanvas.nativeElement.getContext('2d'), {
        type: 'bar',
        data: {
          labels: ageLabels,
          datasets: [{
            label: 'Customers',
            data: ageCounts,
            backgroundColor: '#0f2c59',
            borderRadius: 4
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }

    // 2. Gender Distribution (Pie)
    if (this.genderChartCanvas) {
      const genderLabels = this.demographics.genderDistribution.map(g => g.range);
      const genderCounts = this.demographics.genderDistribution.map(g => g.count);
      this.genderChart = new Chart(this.genderChartCanvas.nativeElement.getContext('2d'), {
        type: 'pie',
        data: {
          labels: genderLabels,
          datasets: [{
            data: genderCounts,
            backgroundColor: ['#3b82f6', '#ec4899', '#94a3b8']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }

    // 3. Income Distribution (Bar)
    if (this.incomeChartCanvas) {
      const incomeLabels = this.demographics.incomeDistribution.map(i => i.range);
      const incomeCounts = this.demographics.incomeDistribution.map(i => i.count);
      this.incomeChart = new Chart(this.incomeChartCanvas.nativeElement.getContext('2d'), {
        type: 'bar',
        data: {
          labels: incomeLabels,
          datasets: [{
            label: 'Customers',
            data: incomeCounts,
            backgroundColor: '#2e7d32',
            borderRadius: 4
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }

    // 4. Country Distribution (Doughnut)
    if (this.countryChartCanvas) {
      const countryLabels = this.demographics.countryDistribution.map(c => c.range);
      const countryCounts = this.demographics.countryDistribution.map(c => c.count);
      this.countryChart = new Chart(this.countryChartCanvas.nativeElement.getContext('2d'), {
        type: 'doughnut',
        data: {
          labels: countryLabels,
          datasets: [{
            data: countryCounts,
            backgroundColor: ['#4f46e5', '#06b6d4', '#f59e0b', '#10b981', '#ef4444']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }
  }
}
