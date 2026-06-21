import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { CampaignComparison } from '../../models/types';

@Component({
  selector: 'app-campaign-comparison',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './campaign-comparison.component.html',
  styleUrls: ['./campaign-comparison.component.css']
})
export class CampaignComparisonComponent implements OnInit {
  campaigns: CampaignComparison[] = [];
  loading = true;
  isDbEmpty = false;

  highestRoi = -9999;
  lowestRoi = 9999;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadComparisonData();
  }

  loadComparisonData() {
    this.loading = true;
    this.apiService.getCampaignComparison().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.campaigns = response.data;
          this.isDbEmpty = this.campaigns.length === 0;

          if (!this.isDbEmpty) {
            // Find highest and lowest ROI values for cell highlighting
            this.campaigns.forEach(c => {
              if (c.roi > this.highestRoi) this.highestRoi = c.roi;
              if (c.roi < this.lowestRoi) this.lowestRoi = c.roi;
            });
          }
        } else {
          this.isDbEmpty = true;
        }
      },
      error: () => {
        this.loading = false;
        this.isDbEmpty = true;
      }
    });
  }
}
