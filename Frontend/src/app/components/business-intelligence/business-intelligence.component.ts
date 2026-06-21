import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-business-intelligence',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './business-intelligence.component.html',
  styleUrls: ['./business-intelligence.component.css']
})
export class BusinessIntelligenceComponent {
  isLoadingCampaign = false;
  isLoadingCustomer = false;
  isLoadingSummary = false;
  isLoadingMonthly = false;
  isLoadingAll = false;
  toastMessage = '';
  toastType: 'success' | 'danger' | 'warning' = 'success';
  exportedPath = '';

  constructor(private apiService: ApiService) {}

  showToast(message: string, type: 'success' | 'danger' | 'warning' = 'success') {
    this.toastMessage = message;
    this.toastType = type;
    setTimeout(() => {
      this.toastMessage = '';
    }, 5000);
  }

  exportCampaign() {
    this.isLoadingCampaign = true;
    this.apiService.exportTableauCampaign().subscribe({
      next: (blob) => {
        this.isLoadingCampaign = false;
        this.triggerBlobDownload(blob, 'campaign_performance.csv');
        this.showToast('Campaign Performance CSV exported successfully!', 'success');
      },
      error: (err) => {
        this.isLoadingCampaign = false;
        console.error('Error exporting campaign performance CSV', err);
        this.showToast('Failed to export campaign performance CSV.', 'danger');
      }
    });
  }

  exportCustomer() {
    this.isLoadingCustomer = true;
    this.apiService.exportTableauCustomer().subscribe({
      next: (blob) => {
        this.isLoadingCustomer = false;
        this.triggerBlobDownload(blob, 'customer_analytics.csv');
        this.showToast('Customer Analytics CSV exported successfully!', 'success');
      },
      error: (err) => {
        this.isLoadingCustomer = false;
        console.error('Error exporting customer analytics CSV', err);
        this.showToast('Failed to export customer analytics CSV.', 'danger');
      }
    });
  }

  exportSummary() {
    this.isLoadingSummary = true;
    this.apiService.exportTableauSummary().subscribe({
      next: (blob) => {
        this.isLoadingSummary = false;
        this.triggerBlobDownload(blob, 'campaign_summary.csv');
        this.showToast('Campaign Summary KPIs CSV exported successfully!', 'success');
      },
      error: (err) => {
        this.isLoadingSummary = false;
        console.error('Error exporting campaign summary CSV', err);
        this.showToast('Failed to export campaign summary CSV.', 'danger');
      }
    });
  }

  exportMonthly() {
    this.isLoadingMonthly = true;
    this.apiService.exportTableauMonthly().subscribe({
      next: (blob) => {
        this.isLoadingMonthly = false;
        this.triggerBlobDownload(blob, 'monthly_revenue.csv');
        this.showToast('Monthly Revenue Trend CSV exported successfully!', 'success');
      },
      error: (err) => {
        this.isLoadingMonthly = false;
        console.error('Error exporting monthly revenue CSV', err);
        this.showToast('Failed to export monthly revenue CSV.', 'danger');
      }
    });
  }

  exportAll() {
    this.isLoadingAll = true;
    this.apiService.exportTableauAll().subscribe({
      next: (res) => {
        this.isLoadingAll = false;
        if (res.success) {
          this.exportedPath = res.data;
          this.showToast('All datasets exported to Tableau/Datasets/ folder successfully!', 'success');
          // Trigger individual file downloads in browser sequentially
          this.triggerAllDownloads();
        } else {
          this.showToast(res.message, 'warning');
        }
      },
      error: (err) => {
        this.isLoadingAll = false;
        console.error('Error exporting all datasets', err);
        this.showToast('Failed to export all datasets together.', 'danger');
      }
    });
  }

  private triggerAllDownloads() {
    // Download each file sequentially to avoid browser popups blocking multiple files
    setTimeout(() => {
      this.apiService.exportTableauCampaign().subscribe(b => this.triggerBlobDownload(b, 'campaign_performance.csv'));
    }, 100);

    setTimeout(() => {
      this.apiService.exportTableauCustomer().subscribe(b => this.triggerBlobDownload(b, 'customer_analytics.csv'));
    }, 400);

    setTimeout(() => {
      this.apiService.exportTableauSummary().subscribe(b => this.triggerBlobDownload(b, 'campaign_summary.csv'));
    }, 700);

    setTimeout(() => {
      this.apiService.exportTableauMonthly().subscribe(b => this.triggerBlobDownload(b, 'monthly_revenue.csv'));
    }, 1000);
  }

  private triggerBlobDownload(blob: Blob, defaultFileName: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = defaultFileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}
