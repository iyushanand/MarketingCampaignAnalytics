import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { ExtendedCampaign } from '../campaign-performance/campaign-performance.component';

@Component({
  selector: 'app-campaign-comparison',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './campaign-comparison.component.html',
  styleUrls: ['./campaign-comparison.component.css']
})
export class CampaignComparisonComponent implements OnInit {
  campaigns: ExtendedCampaign[] = [];
  loading = true;
  isDbEmpty = false;

  // Highlight metrics
  highestRevenueCamp: ExtendedCampaign | null = null;
  highestRoiCamp: ExtendedCampaign | null = null;
  highestConversionCamp: ExtendedCampaign | null = null;
  lowestRoiCamp: ExtendedCampaign | null = null;

  // Side-by-side selections
  selectedCampAId: string = '';
  selectedCampBId: string = '';
  campA: ExtendedCampaign | null = null;
  campB: ExtendedCampaign | null = null;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadComparisonData();
  }

  loadComparisonData() {
    this.loading = true;
    this.apiService.getCampaigns().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data && response.data.length > 0) {
          const rawCampaigns = response.data;
          
          this.campaigns = rawCampaigns.map(c => {
            const spend = c.spend;
            const revenue = c.revenue;
            const roi = spend > 0 ? (revenue - spend) / spend : 0;
            const roas = spend > 0 ? revenue / spend : 0;
            const ctr = c.impressions > 0 ? c.clicks / c.impressions : 0;
            const conversionRate = c.impressions > 0 ? c.conversions / c.impressions : 0;
            
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
              responseRate: 0, // default, not required here
              status: c.status,
              startDate: c.startDate,
              endDate: c.endDate
            };
          });

          this.isDbEmpty = this.campaigns.length === 0;

          if (!this.isDbEmpty) {
            this.identifyHighlights();
            
            // Default selections
            if (this.campaigns.length >= 2) {
              this.selectedCampAId = this.campaigns[0].campaignId.toString();
              this.selectedCampBId = this.campaigns[1].campaignId.toString();
              this.onCampaignSelect('A');
              this.onCampaignSelect('B');
            }
          }
        } else {
          this.isDbEmpty = true;
        }
      },
      error: () => {
        this.loading = false;
        this.isDbEmpty = true;
        this.toastService.error('Failed to retrieve campaign comparative metrics.');
      }
    });
  }

  identifyHighlights() {
    if (this.campaigns.length === 0) return;
    
    // Sort and grab extremes
    const sortedRevenue = [...this.campaigns].sort((a, b) => b.revenue - a.revenue);
    this.highestRevenueCamp = sortedRevenue[0];
    
    const sortedRoi = [...this.campaigns].sort((a, b) => b.roi - a.roi);
    this.highestRoiCamp = sortedRoi[0];
    this.lowestRoiCamp = sortedRoi[sortedRoi.length - 1];
    
    const sortedConv = [...this.campaigns].sort((a, b) => b.conversionRate - a.conversionRate);
    this.highestConversionCamp = sortedConv[0];
  }

  onCampaignSelect(selector: 'A' | 'B') {
    if (selector === 'A') {
      this.campA = this.campaigns.find(c => c.campaignId.toString() === this.selectedCampAId) || null;
    } else {
      this.campB = this.campaigns.find(c => c.campaignId.toString() === this.selectedCampBId) || null;
    }
  }
}
