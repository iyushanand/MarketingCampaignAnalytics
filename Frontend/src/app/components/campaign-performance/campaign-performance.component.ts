import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { CampaignPerformance } from '../../models/types';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-campaign-performance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './campaign-performance.component.html',
  styleUrls: ['./campaign-performance.component.css']
})
export class CampaignPerformanceComponent implements OnInit, AfterViewInit {
  campaigns: CampaignPerformance[] = [];
  filteredCampaigns: CampaignPerformance[] = [];
  paginatedCampaigns: CampaignPerformance[] = [];
  
  loading = true;
  isDbEmpty = false;

  // Filters & Search
  searchText = '';
  selectedChannel = '';
  channelsList: string[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 5;
  totalPages = 1;

  @ViewChild('spendRevenueCanvas') spendRevenueCanvas!: ElementRef;
  private chart: Chart | null = null;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadPerformanceData();
  }

  ngAfterViewInit() {
    // Rendered dynamically
  }

  loadPerformanceData() {
    this.loading = true;
    this.apiService.getCampaignPerformance().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.campaigns = response.data;
          this.isDbEmpty = this.campaigns.length === 0;
          
          if (!this.isDbEmpty) {
            // Deduplicate channels for filter dropdown
            const channels = this.campaigns.map(c => c.marketingChannel || (c as any).channel).filter(Boolean);
            this.channelsList = Array.from(new Set(channels));
            
            this.applyFilters();
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

  applyFilters() {
    this.filteredCampaigns = this.campaigns.filter(c => {
      const channelMatch = !this.selectedChannel || 
                           (c.marketingChannel || (c as any).channel || '').toLowerCase() === this.selectedChannel.toLowerCase();
      const searchMatch = !this.searchText || 
                          c.campaignName.toLowerCase().includes(this.searchText.toLowerCase());
      return channelMatch && searchMatch;
    });

    this.currentPage = 1;
    this.totalPages = Math.ceil(this.filteredCampaigns.length / this.pageSize) || 1;
    this.updatePagination();

    // Replot chart based on filtered items
    setTimeout(() => {
      this.renderChart();
    }, 0);
  }

  updatePagination() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    this.paginatedCampaigns = this.filteredCampaigns.slice(startIndex, startIndex + this.pageSize);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePagination();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePagination();
    }
  }

  renderChart() {
    if (!this.spendRevenueCanvas) return;
    if (this.chart) {
      this.chart.destroy();
    }

    const labels = this.filteredCampaigns.map(c => c.campaignName);
    const spends = this.filteredCampaigns.map(c => c.spend);
    const revenues = this.filteredCampaigns.map(c => c.revenue);

    const ctx = this.spendRevenueCanvas.nativeElement.getContext('2d');
    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Spend ($)',
            data: spends,
            backgroundColor: '#c92a2a', // deep red
            borderRadius: 4,
          },
          {
            label: 'Revenue ($)',
            data: revenues,
            backgroundColor: '#0f2c59', // deep blue
            borderRadius: 4,
          }
        ]
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
}
