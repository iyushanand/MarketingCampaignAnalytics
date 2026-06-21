import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { ReportFileDto } from '../../models/types';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {
  reportsList: ReportFileDto[] = [];
  isLoadingList = false;
  isLoadingExcel = false;
  isLoadingPdf = false;
  toastMessage = '';
  toastType: 'success' | 'danger' | 'warning' = 'success';

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.fetchReportsHistory();
  }

  showToast(message: string, type: 'success' | 'danger' | 'warning' = 'success') {
    this.toastMessage = message;
    this.toastType = type;
    setTimeout(() => {
      this.toastMessage = '';
    }, 5000);
  }

  fetchReportsHistory() {
    this.isLoadingList = true;
    this.apiService.getReportsList().subscribe({
      next: (res) => {
        this.isLoadingList = false;
        if (res.success) {
          this.reportsList = res.data;
        } else {
          this.showToast(res.message, 'warning');
        }
      },
      error: (err) => {
        this.isLoadingList = false;
        console.error('Error fetching reports history', err);
        this.showToast('Failed to retrieve reports archive list.', 'danger');
      }
    });
  }

  generateExcelReport() {
    this.isLoadingExcel = true;
    this.apiService.generateExcelReport().subscribe({
      next: (blob) => {
        this.isLoadingExcel = false;
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        this.triggerBlobDownload(blob, `Marketing_Report_${timestamp}.xlsx`);
        this.showToast('Excel report generated and downloaded successfully!', 'success');
        this.fetchReportsHistory();
      },
      error: (err) => {
        this.isLoadingExcel = false;
        console.error('Error generating Excel report', err);
        this.showToast('Error generating Excel report. Ensure dataset is seeded.', 'danger');
      }
    });
  }

  generatePdfReport() {
    this.isLoadingPdf = true;
    this.apiService.generatePdfReport().subscribe({
      next: (blob) => {
        this.isLoadingPdf = false;
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        this.triggerBlobDownload(blob, `Marketing_Report_${timestamp}.pdf`);
        this.showToast('PDF report generated and downloaded successfully!', 'success');
        this.fetchReportsHistory();
      },
      error: (err) => {
        this.isLoadingPdf = false;
        console.error('Error generating PDF report', err);
        this.showToast('Error generating PDF report. Ensure dataset is seeded.', 'danger');
      }
    });
  }

  downloadReport(report: ReportFileDto) {
    this.apiService.downloadArchivedReport(report.fileName).subscribe({
      next: (blob) => {
        this.triggerBlobDownload(blob, report.fileName);
        this.showToast(`Downloaded archived copy of ${report.fileName}`, 'success');
      },
      error: (err) => {
        console.error('Error downloading archived report', err);
        this.showToast('Failed to download archived report copy.', 'danger');
      }
    });
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
