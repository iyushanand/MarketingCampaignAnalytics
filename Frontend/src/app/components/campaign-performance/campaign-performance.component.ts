import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { CampaignDto, CampaignEffectiveness } from '../../models/types';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

export interface ExtendedCampaign {
  campaignId: number;
  campaignName: string;
  campaignType: string;
  marketingChannel: string;
  budget: number;
  spend: number;
  revenue: number;
  roi: number;
  roas: number;
  impressions: number;
  clicks: number;
  ctr: number;
  conversions: number;
  conversionRate: number;
  responseRate: number;
  status: string;
  startDate: string;
  endDate: string;
}

export interface ChannelAnalysis {
  channelName: string;
  totalSpend: number;
  totalRevenue: number;
  avgRoi: number;
  avgCtr: number;
  avgConversionRate: number;
}

@Component({
  selector: 'app-campaign-performance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './campaign-performance.component.html',
  styleUrls: ['./campaign-performance.component.css']
})
export class CampaignPerformanceComponent implements OnInit, AfterViewInit {
  campaigns: ExtendedCampaign[] = [];
  filteredCampaigns: ExtendedCampaign[] = [];
  paginatedCampaigns: ExtendedCampaign[] = [];
  
  channelsList: string[] = [];
  channelAnalysisList: ChannelAnalysis[] = [];
  
  loading = true;
  isDbEmpty = false;

  // Filters & Search
  searchText = '';
  selectedChannel = '';
  selectedStatus = '';
  selectedCampaignType = '';
  campaignTypesList: string[] = [];

  // Sorting
  sortBy: 'revenue' | 'roi' | 'conversionRate' | '' = '';
  sortOrder: 'asc' | 'desc' = 'desc';

  // Pagination
  currentPage = 1;
  pageSize = 5;
  totalPages = 1;

  // KPI Summaries
  summaryKpis = {
    totalCampaigns: 0,
    activeCampaigns: 0,
    completedCampaigns: 0,
    totalSpend: 0,
    totalRevenue: 0,
    avgRoi: 0,
    avgConversionRate: 0,
    avgResponseRate: 0
  };

  // Best/Lowest Channels
  bestChannelName = 'N/A';
  lowestChannelName = 'N/A';

  // Business Insights list
  businessInsights: string[] = [];

  // Canvas ViewChild Elements
  @ViewChild('spendRevenueCanvas') spendRevenueCanvas!: ElementRef;
  @ViewChild('revenueChannelCanvas') revenueChannelCanvas!: ElementRef;
  @ViewChild('performanceTrendCanvas') performanceTrendCanvas!: ElementRef;

  private spendChart: Chart | null = null;
  private channelChart: Chart | null = null;
  private trendChart: Chart | null = null;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadAllCampaignData();
  }

  ngAfterViewInit() {
    // Dynamic chart instantiation triggered on data load
  }

  loadAllCampaignData() {
    this.loading = true;
    
    // Concurrently fetch raw campaign information and response effectiveness rates
    this.apiService.getCampaigns().subscribe({
      next: (campResponse) => {
        if (campResponse.success && campResponse.data && campResponse.data.length > 0) {
          const rawCampaigns = campResponse.data;
          
          this.apiService.getCampaignEffectiveness().subscribe({
            next: (effResponse) => {
              this.loading = false;
              const effData = effResponse.success && effResponse.data ? effResponse.data : [];
              
              // Map and merge datasets
              this.campaigns = rawCampaigns.map(c => {
                const effMatch = effData.find(e => e.campaignName === c.campaignName);
                const spend = c.spend;
                const revenue = c.revenue;
                const roi = spend > 0 ? (revenue - spend) / spend : 0;
                const roas = spend > 0 ? revenue / spend : 0;
                const ctr = c.impressions > 0 ? c.clicks / c.impressions : 0;
                const conversionRate = c.impressions > 0 ? c.conversions / c.impressions : 0;
                const responseRate = effMatch ? effMatch.responseRate : 0;
                
                return {
                  campaignId: c.campaignId,
                  campaignName: c.campaignName,
                  campaignType: c.campaignType,
                  marketingChannel: c.marketingChannel,
                  budget: c.budget,
                  spend: spend,
                  revenue: revenue,
                  roi: roi,
                  roas: roas,
                  impressions: c.impressions,
                  clicks: c.clicks,
                  ctr: ctr,
                  conversions: c.conversions,
                  conversionRate: conversionRate,
                  responseRate: responseRate,
                  status: c.status,
                  startDate: c.startDate,
                  endDate: c.endDate
                };
              });

              this.isDbEmpty = this.campaigns.length === 0;

              if (!this.isDbEmpty) {
                const channels = this.campaigns.map(c => c.marketingChannel).filter(Boolean);
                this.channelsList = Array.from(new Set(channels));
                
                const types = this.campaigns.map(c => c.campaignType).filter(Boolean);
                this.campaignTypesList = Array.from(new Set(types));
                
                this.calculateKpiSummaries();
                this.calculateChannelAnalysis();
                this.generateInsights();
                this.applyFilters();
              }
            },
            error: () => {
              this.loading = false;
              this.isDbEmpty = true;
              this.toastService.error('Error fetching campaign response rates.');
            }
          });
        } else {
          this.loading = false;
          this.isDbEmpty = true;
        }
      },
      error: () => {
        this.loading = false;
        this.isDbEmpty = true;
        this.toastService.error('Failed to load campaigns from local SQL Server.');
      }
    });
  }

  calculateKpiSummaries() {
    const totalCamp = this.campaigns.length;
    const active = this.campaigns.filter(c => c.status.toLowerCase() === 'active').length;
    const completed = this.campaigns.filter(c => c.status.toLowerCase() === 'completed').length;
    const totalSpend = this.campaigns.reduce((acc, c) => acc + c.spend, 0);
    const totalRev = this.campaigns.reduce((acc, c) => acc + c.revenue, 0);
    
    // Average metrics
    const avgRoi = this.campaigns.reduce((acc, c) => acc + c.roi, 0) / totalCamp;
    const avgConv = this.campaigns.reduce((acc, c) => acc + c.conversionRate, 0) / totalCamp;
    const avgResp = this.campaigns.reduce((acc, c) => acc + c.responseRate, 0) / totalCamp;

    this.summaryKpis = {
      totalCampaigns: totalCamp,
      activeCampaigns: active,
      completedCampaigns: completed,
      totalSpend: totalSpend,
      totalRevenue: totalRev,
      avgRoi: avgRoi,
      avgConversionRate: avgConv,
      avgResponseRate: avgResp
    };
  }

  calculateChannelAnalysis() {
    const channelGroups: { [key: string]: ExtendedCampaign[] } = {};
    this.campaigns.forEach(c => {
      if (!channelGroups[c.marketingChannel]) {
        channelGroups[c.marketingChannel] = [];
      }
      channelGroups[c.marketingChannel].push(c);
    });

    this.channelAnalysisList = Object.keys(channelGroups).map(channelName => {
      const list = channelGroups[channelName];
      const count = list.length;
      const tSpend = list.reduce((acc, c) => acc + c.spend, 0);
      const tRevenue = list.reduce((acc, c) => acc + c.revenue, 0);
      const avgRoi = list.reduce((acc, c) => acc + c.roi, 0) / count;
      const avgCtr = list.reduce((acc, c) => acc + c.ctr, 0) / count;
      const avgConv = list.reduce((acc, c) => acc + c.conversionRate, 0) / count;

      return {
        channelName,
        totalSpend: tSpend,
        totalRevenue: tRevenue,
        avgRoi,
        avgCtr,
        avgConversionRate: avgConv
      };
    });

    if (this.channelAnalysisList.length > 0) {
      // Best channel has the highest average ROI
      const sortedByRoi = [...this.channelAnalysisList].sort((a, b) => b.avgRoi - a.avgRoi);
      this.bestChannelName = sortedByRoi[0].channelName;
      this.lowestChannelName = sortedByRoi[sortedByRoi.length - 1].channelName;
    }
  }

  generateInsights() {
    const insights: string[] = [];
    if (this.campaigns.length === 0) return;

    const formatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

    // Insight 1: Compare Best ROI campaign with average revenue
    const sortedByRoi = [...this.campaigns].sort((a, b) => b.roi - a.roi);
    const bestRoiCamp = sortedByRoi[0];
    const avgRevenue = this.summaryKpis.totalRevenue / this.summaryKpis.totalCampaigns;
    if (bestRoiCamp && avgRevenue > 0) {
      const revenueDifferencePct = Math.round(((bestRoiCamp.revenue - avgRevenue) / avgRevenue) * 100);
      const relWord = revenueDifferencePct >= 0 ? 'more' : 'less';
      insights.push(`The highest ROI campaign is '${bestRoiCamp.campaignName}' (${Math.round(bestRoiCamp.roi * 100)}% ROI), generating ${Math.abs(revenueDifferencePct)}% ${relWord} revenue than the portfolio average.`);
    }

    // Insight 2: Channel comparison
    const emailData = this.channelAnalysisList.find(c => c.channelName.toLowerCase() === 'email');
    const socialData = this.channelAnalysisList.find(c => c.channelName.toLowerCase() === 'social media');
    if (emailData && socialData) {
      const diff = Math.round((emailData.avgRoi - socialData.avgRoi) * 100);
      const comparisonWord = diff >= 0 ? 'outperform' : 'underperform';
      insights.push(`Email campaigns consistently ${comparisonWord} Social Media channels by ${Math.abs(diff)}% average ROI difference.`);
    }

    // Insight 3: High spend / low conversion check
    const sortedBySpend = [...this.campaigns].sort((a, b) => b.spend - a.spend);
    const highestSpender = sortedBySpend[0];
    if (highestSpender && highestSpender.conversionRate < this.summaryKpis.avgConversionRate) {
      insights.push(`Campaign '${highestSpender.campaignName}' has the highest spend (${formatter.format(highestSpender.spend)}), but yields poor conversion rates (${(highestSpender.conversionRate * 100).toFixed(2)}%) below the average.`);
    }

    // Insight 4: Additional budget recommendation
    const highRoiCompleted = this.campaigns.find(c => c.roi > 0.40 && c.status.toLowerCase() === 'completed');
    if (highRoiCompleted) {
      insights.push(`Campaign '${highRoiCompleted.campaignName}' has achieved high margins (ROI: ${Math.round(highRoiCompleted.roi * 100)}%). Recommend allocating additional budget in future quarters.`);
    } else if (bestRoiCamp) {
      insights.push(`Campaign '${bestRoiCamp.campaignName}' displays leading performance metrics. Recommend scaling spend to expand conversions.`);
    }

    this.businessInsights = insights.slice(0, 5);
  }

  applyFilters() {
    this.filteredCampaigns = this.campaigns.filter(c => {
      const searchMatch = !this.searchText || 
                          c.campaignName.toLowerCase().includes(this.searchText.toLowerCase());
      const channelMatch = !this.selectedChannel || 
                           c.marketingChannel.toLowerCase() === this.selectedChannel.toLowerCase();
      const statusMatch = !this.selectedStatus || 
                          c.status.toLowerCase() === this.selectedStatus.toLowerCase();
      const typeMatch = !this.selectedCampaignType ||
                        c.campaignType.toLowerCase() === this.selectedCampaignType.toLowerCase();
      return searchMatch && channelMatch && statusMatch && typeMatch;
    });

    // Apply Sorting
    if (this.sortBy) {
      this.filteredCampaigns.sort((a, b) => {
        const valA = a[this.sortBy as keyof ExtendedCampaign] as number;
        const valB = b[this.sortBy as keyof ExtendedCampaign] as number;
        return this.sortOrder === 'asc' ? valA - valB : valB - valA;
      });
    }

    this.currentPage = 1;
    this.totalPages = Math.ceil(this.filteredCampaigns.length / this.pageSize) || 1;
    this.updatePagination();

    // Re-trigger chart rendering
    setTimeout(() => {
      this.renderCharts();
    }, 0);
  }

  setSort(field: 'revenue' | 'roi' | 'conversionRate') {
    if (this.sortBy === field) {
      this.sortOrder = this.sortOrder === 'desc' ? 'asc' : 'desc';
    } else {
      this.sortBy = field;
      this.sortOrder = 'desc';
    }
    this.applyFilters();
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

  renderCharts() {
    // 1. Spend vs Revenue (Bar Chart)
    if (this.spendRevenueCanvas) {
      if (this.spendChart) this.spendChart.destroy();
      const ctx = this.spendRevenueCanvas.nativeElement.getContext('2d');
      
      const labels = this.filteredCampaigns.map(c => c.campaignName);
      const spends = this.filteredCampaigns.map(c => c.spend);
      const revenues = this.filteredCampaigns.map(c => c.revenue);

      this.spendChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels,
          datasets: [
            { label: 'Spend ($)', data: spends, backgroundColor: '#c92a2a', borderRadius: 4 },
            { label: 'Revenue ($)', data: revenues, backgroundColor: '#0f2c59', borderRadius: 4 }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            y: { beginAtZero: true, ticks: { callback: (val) => '$' + Number(val).toLocaleString() } }
          }
        }
      });
    }

    // 2. Revenue by Marketing Channel (Pie Chart)
    if (this.revenueChannelCanvas && this.channelAnalysisList.length > 0) {
      if (this.channelChart) this.channelChart.destroy();
      const ctx = this.revenueChannelCanvas.nativeElement.getContext('2d');

      const labels = this.channelAnalysisList.map(c => c.channelName);
      const revenues = this.channelAnalysisList.map(c => c.totalRevenue);

      this.channelChart = new Chart(ctx, {
        type: 'pie',
        data: {
          labels,
          datasets: [{
            data: revenues,
            backgroundColor: ['#0f2c59', '#3b82f6', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { position: 'bottom', labels: { boxWidth: 12, padding: 8 } }
          }
        }
      });
    }

    // 3. Campaign Performance Trend (Line Chart - ROI over start dates)
    if (this.performanceTrendCanvas) {
      if (this.trendChart) this.trendChart.destroy();
      const ctx = this.performanceTrendCanvas.nativeElement.getContext('2d');

      // Sort chronological
      const cronCampaigns = [...this.filteredCampaigns].sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime());
      
      const labels = cronCampaigns.map(c => c.campaignName);
      const rois = cronCampaigns.map(c => Math.round(c.roi * 100));

      this.trendChart = new Chart(ctx, {
        type: 'line',
        data: {
          labels,
          datasets: [{
            label: 'ROI (%)',
            data: rois,
            borderColor: '#10b981',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            fill: true,
            tension: 0.3,
            borderWidth: 2
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            y: { ticks: { callback: (val) => val + '%' } }
          }
        }
      });
    }
  }
}
