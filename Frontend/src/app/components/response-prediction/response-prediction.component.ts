import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { PredictionRequest, PredictionResponse, PredictionMetrics } from '../../models/types';

@Component({
  selector: 'app-response-prediction',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './response-prediction.component.html',
  styleUrls: ['./response-prediction.component.css']
})
export class ResponsePredictionComponent implements OnInit {
  // Input fields
  age: number = 35;
  income: number = 55000;
  education: string = 'Graduation';
  totalPurchases: number = 8;
  averageSpend: number = 50;
  campaignChannel: string = 'Email';

  // Options
  educationOptions: string[] = ['Graduation', 'PhD', 'Master', 'Basic', '2n Cycle'];
  channelOptions: string[] = ['Email', 'SMS', 'Social Media', 'Google Search Ads', 'Display Ads'];

  // State & Results
  submitting = false;
  training = false;
  predictionResult: PredictionResponse | null = null;
  metrics: PredictionMetrics | null = null;
  errorMsg = '';
  successMsg = '';

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadMetrics();
  }

  loadMetrics() {
    this.apiService.getPredictionMetrics().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.metrics = res.data;
        }
      },
      error: () => {
        // Fallback or silent ignore in case model is untrained initially
      }
    });
  }

  predict() {
    this.submitting = true;
    this.errorMsg = '';
    
    const request: PredictionRequest = {
      age: this.age,
      income: this.income,
      education: this.education,
      totalPurchases: this.totalPurchases,
      averageSpend: this.averageSpend,
      campaignChannel: this.campaignChannel
    };

    this.apiService.predictResponse(request).subscribe({
      next: (res) => {
        this.submitting = false;
        if (res.success && res.data) {
          this.predictionResult = res.data;
        } else {
          this.errorMsg = res.message || 'Prediction failed. Verify dataset is seeded.';
        }
      },
      error: (err) => {
        this.submitting = false;
        this.errorMsg = 'Error communicating with prediction service.';
      }
    });
  }

  triggerTraining() {
    this.training = true;
    this.errorMsg = '';
    this.successMsg = '';
    this.predictionResult = null;

    this.apiService.trainPredictionModel().subscribe({
      next: (res) => {
        this.training = false;
        if (res.success) {
          this.successMsg = 'Model retrained successfully.';
          this.loadMetrics();
        } else {
          this.errorMsg = res.message || 'Failed to retrain model.';
        }
      },
      error: () => {
        this.training = false;
        this.errorMsg = 'Error retraining model in background.';
      }
    });
  }

  getConfidenceBadgeClass(level: string): string {
    if (!level) return 'bg-secondary';
    switch (level.toLowerCase()) {
      case 'high': return 'bg-success';
      case 'medium': return 'bg-warning text-dark';
      case 'low': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }
}
