import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { CampaignEffectiveness } from '../../models/types';

@Component({
  selector: 'app-campaign-effectiveness',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './campaign-effectiveness.component.html',
  styleUrls: ['./campaign-effectiveness.component.css']
})
export class CampaignEffectivenessComponent implements OnInit {
  campaigns: CampaignEffectiveness[] = [];
  loading = true;
  isDbEmpty = false;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadEffectivenessData();
  }

  loadEffectivenessData() {
    this.loading = true;
    this.apiService.getCampaignEffectiveness().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.campaigns = response.data;
          this.isDbEmpty = this.campaigns.length === 0;
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
