import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { DashboardKpis, CampaignDto } from '../../models/types';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, AfterViewInit {
  kpis: DashboardKpis | null = null;
  topCampaigns: CampaignDto[] = [];
  loading = true;
  isDbEmpty = false;

  @ViewChild('revenueTrendCanvas') revenueTrendCanvas!: ElementRef;
  @ViewChild('topCampaignsCanvas') topCampaignsCanvas!: ElementRef;

  private revenueChart: Chart | null = null;
  private campaignsChart: Chart | null = null;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadKpis();
  }

  ngAfterViewInit() {
    // Canvas elements will be rendered dynamically after API calls return data
  }

  loadKpis() {
    this.loading = true;
    this.apiService.getDashboardKpis().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.kpis = response.data;
          this.isDbEmpty = this.kpis.totalCustomers === 0;
          if (!this.isDbEmpty) {
            this.loadTrend();
            this.loadTopCampaigns();
          } else {
            this.loading = false;
          }
        } else {
          this.isDbEmpty = true;
          this.loading = false;
        }
      },
      error: () => {
        this.loading = false;
        this.isDbEmpty = true;
      }
    });
  }

  loadTrend() {
    this.apiService.getRevenueTrend().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const trendData = response.data;
          const labels = trendData.map(t => t.month);
          const revenues = trendData.map(t => t.revenue);
          
          setTimeout(() => {
            this.renderRevenueChart(labels, revenues);
          }, 0);
        }
      }
    });
  }

  loadTopCampaigns() {
    this.apiService.getTopCampaigns().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.topCampaigns = response.data;
          const labels = this.topCampaigns.map(c => c.campaignName);
          const rois = this.topCampaigns.map(c => c.spend > 0 ? (c.revenue - c.spend) / c.spend : 0);
          
          setTimeout(() => {
            this.renderCampaignsChart(labels, rois);
          }, 0);
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  renderRevenueChart(labels: string[], revenues: number[]) {
    if (!this.revenueTrendCanvas) return;
    if (this.revenueChart) {
      this.revenueChart.destroy();
    }
    const ctx = this.revenueTrendCanvas.nativeElement.getContext('2d');
    this.revenueChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [{
          label: 'Revenue ($)',
          data: revenues,
          borderColor: '#0f2c59',
          backgroundColor: 'rgba(15, 44, 89, 0.1)',
          borderWidth: 2,
          fill: true,
          tension: 0.3
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value) => '$' + Number(value).toLocaleString()
            }
          }
        }
      }
    });
  }

  renderCampaignsChart(labels: string[], rois: number[]) {
    if (!this.topCampaignsCanvas) return;
    if (this.campaignsChart) {
      this.campaignsChart.destroy();
    }
    const ctx = this.topCampaignsCanvas.nativeElement.getContext('2d');
    this.campaignsChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'ROI (%)',
          data: rois.map(r => Math.round(r * 100)),
          backgroundColor: '#2e7d32',
          borderRadius: 4,
          barThickness: 30
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value) => value + '%'
            }
          }
        }
      }
    });
  }
}
