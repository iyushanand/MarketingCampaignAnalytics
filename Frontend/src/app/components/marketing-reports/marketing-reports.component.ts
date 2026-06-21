import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-marketing-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './marketing-reports.component.html',
  styleUrls: ['./marketing-reports.component.css']
})
export class MarketingReportsComponent implements OnInit {
  reportTypes = [
    { value: 'campaign', label: 'Campaign Performance Report' },
    { value: 'customer', label: 'Customer Insights Report' },
    { value: 'channel', label: 'Channel Performance Report' },
    { value: 'monthly', label: 'Monthly Revenue Report' }
  ];
  
  selectedReport = 'campaign';
  reportData: any[] = [];
  filteredData: any[] = [];
  paginatedData: any[] = [];
  
  headers: string[] = [];
  displayHeaders: string[] = [];
  
  loading = false;
  isDbEmpty = false;

  // Search & Pagination
  searchText = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.fetchReport();
  }

  fetchReport() {
    this.loading = true;
    this.reportData = [];
    this.filteredData = [];
    this.paginatedData = [];
    this.headers = [];
    this.displayHeaders = [];

    this.apiService.getMarketingReportData(this.selectedReport).subscribe({
      next: (data) => {
        this.loading = false;
        if (data && data.length > 0) {
          this.reportData = data;
          this.isDbEmpty = false;
          
          // Extract headers
          this.headers = Object.keys(data[0]);
          this.displayHeaders = this.headers.map(h => this.formatHeader(h));
          
          this.applyFilters();
        } else {
          this.isDbEmpty = true;
          this.toastService.warning('No report records found. Please seed the database first.');
        }
      },
      error: () => {
        this.loading = false;
        this.isDbEmpty = true;
        this.toastService.error('Error fetching report data from API.');
      }
    });
  }

  applyFilters() {
    this.filteredData = this.reportData.filter(row => {
      if (!this.searchText) return true;
      return this.headers.some(header => {
        const val = row[header];
        return val && val.toString().toLowerCase().includes(this.searchText.toLowerCase());
      });
    });

    this.currentPage = 1;
    this.totalPages = Math.ceil(this.filteredData.length / this.pageSize) || 1;
    this.updatePagination();
  }

  updatePagination() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    this.paginatedData = this.filteredData.slice(startIndex, startIndex + this.pageSize);
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

  private formatHeader(val: string): string {
    // Converts camelCase (e.g. campaignName) to Title Case (e.g. Campaign Name)
    const result = val.replace(/([A-Z])/g, " $1");
    return result.charAt(0).toUpperCase() + result.slice(1);
  }
}
