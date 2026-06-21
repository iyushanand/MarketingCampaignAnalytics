import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-upload-dataset',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-dataset.component.html',
  styleUrls: ['./upload-dataset.component.css']
})
export class UploadDatasetComponent {
  selectedFile: File | null = null;
  loading = false;
  uploadSummary: any = null;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService
  ) {}

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.toastService.info(`Selected file: ${file.name}`);
    }
  }

  loadSample() {
    this.loading = true;
    this.uploadSummary = null;
    this.apiService.loadSampleDataset().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success) {
          this.toastService.success(response.message || 'Sample dataset loaded successfully!');
          this.uploadSummary = {
            customersImported: 2240,
            campaignsImported: 6,
            responsesImported: 13440,
            status: 'Success',
            message: 'Bundled Kaggle Customer Personality dataset was loaded into SQL Server.'
          };
        } else {
          this.toastService.error(response.message || 'Failed to load sample dataset.');
        }
      },
      error: (err) => {
        this.loading = false;
        this.toastService.error('Error connecting to backend database server.');
      }
    });
  }

  uploadCsv() {
    if (!this.selectedFile) {
      this.toastService.warning('Please select a CSV file first.');
      return;
    }

    this.loading = true;
    this.uploadSummary = null;
    this.apiService.uploadCsv(this.selectedFile).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success) {
          this.toastService.success(response.message || 'CSV file uploaded and parsed successfully!');
          this.uploadSummary = response.data;
        } else {
          this.toastService.error(response.message || 'Failed to process CSV file.');
        }
      },
      error: (err) => {
        this.loading = false;
        this.toastService.error(err.error?.message || 'Error uploading CSV. Check file format/size.');
      }
    });
  }
}
