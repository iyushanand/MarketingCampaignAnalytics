import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { forkJoin } from 'rxjs';

export interface ExtendedEffectiveness {
  campaignName: string;
  spend: number;
  revenue: number;
  roi: number;
  conversionRate: number;
  responseRate: number;
  status: string;
  isBestRoi: boolean;
  isWorstRoi: boolean;
  isBestResponse: boolean;
}

@Component({
  selector: 'app-campaign-effectiveness',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './campaign-effectiveness.component.html',
  styleUrls: ['./campaign-effectiveness.component.css']
})
export class CampaignEffectivenessComponent implements OnInit {
  campaigns: ExtendedEffectiveness[] = [];
  loading = true;
  isDbEmpty = false;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadEffectivenessData();
  }

  loadEffectivenessData() {
    this.loading = true;
    
    forkJoin({
      camps: this.apiService.getCampaigns(),
      effs: this.apiService.getCampaignEffectiveness()
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.camps.success && res.camps.data && res.camps.data.length > 0) {
          const rawCamps = res.camps.data;
          const effsData = res.effs.success && res.effs.data ? res.effs.data : [];
          
          this.campaigns = effsData.map(e => {
            const campMatch = rawCamps.find(c => c.campaignName === e.campaignName);
            return {
              campaignName: e.campaignName,
              spend: e.spend,
              revenue: e.revenue,
              roi: e.roi,
              conversionRate: e.conversionRate,
              responseRate: e.responseRate,
              status: campMatch ? campMatch.status : 'Unknown',
              isBestRoi: e.isBestRoi,
              isWorstRoi: e.isWorstRoi,
              isBestResponse: e.isBestResponse
            };
          });

          this.isDbEmpty = this.campaigns.length === 0;
        } else {
          this.isDbEmpty = true;
        }
      },
      error: () => {
        this.loading = false;
        this.isDbEmpty = true;
        this.toastService.error('Failed to load campaign effectiveness ratings.');
      }
    });
  }

  getEffectivenessBadge(roi: number): { text: string; class: string } {
    if (roi >= 0.60) {
      return { text: 'Excellent', class: 'bg-success' };
    } else if (roi >= 0.30) {
      return { text: 'Good', class: 'bg-primary' };
    } else if (roi >= 0.0) {
      return { text: 'Average', class: 'bg-warning text-dark' };
    } else {
      return { text: 'Needs Improvement', class: 'bg-danger' };
    }
  }
}
