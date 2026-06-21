import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { 
  CustomerAnalytics, 
  CustomerPersona, 
  CustomerDemographics, 
  CustomerDto 
} from '../../models/types';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-customer-insights',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-insights.component.html',
  styleUrls: ['./customer-insights.component.css']
})
export class CustomerInsightsComponent implements OnInit, AfterViewInit {
  // Data
  summary: CustomerAnalytics | null = null;
  personas: CustomerPersona[] = [];
  analytics: CustomerDemographics | null = null;
  
  // Table Customer list
  customers: CustomerDto[] = [];
  filteredCustomers: CustomerDto[] = [];
  paginatedCustomers: CustomerDto[] = [];

  loading = true;
  isDbEmpty = false;

  // Filters & Search
  searchText = '';
  selectedCountry = '';
  selectedEducation = '';
  selectedSegment = '';
  
  countriesList: string[] = [];
  educationList: string[] = [];
  segmentsList: string[] = ['High Value', 'Medium Value', 'Low Value'];

  // Pagination
  currentPage = 1;
  pageSize = 5;
  totalPages = 1;

  // Insights List
  businessInsights: string[] = [];

  // Chart Canvas View Childs
  @ViewChild('ageGenderCanvas') ageGenderCanvas!: ElementRef;
  @ViewChild('eduCountryCanvas') eduCountryCanvas!: ElementRef;
  @ViewChild('incomeSpendingCanvas') incomeSpendingCanvas!: ElementRef;
  @ViewChild('ageSpendingCanvas') ageSpendingCanvas!: ElementRef;
  @ViewChild('eduResponseCanvas') eduResponseCanvas!: ElementRef;
  @ViewChild('countryResponseCanvas') countryResponseCanvas!: ElementRef;

  private charts: Chart[] = [];

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadAllCustomerData();
  }

  ngAfterViewInit() {
    // Charts will render once API finishes and binds canvases
  }

  loadAllCustomerData() {
    this.loading = true;
    
    // Fetch customer summary KPIs, personas, and demographic analysis concurrently
    this.apiService.getCustomerSummary().subscribe({
      next: (summaryRes) => {
        if (summaryRes.success && summaryRes.data && summaryRes.data.totalCustomers > 0) {
          this.summary = summaryRes.data;
          
          this.apiService.getCustomerPersonas().subscribe({
            next: (personasRes) => {
              this.personas = personasRes.success && personasRes.data ? personasRes.data : [];
              
              this.apiService.getCustomerAnalytics().subscribe({
                next: (analyticsRes) => {
                  this.analytics = analyticsRes.success && analyticsRes.data ? analyticsRes.data : null;
                  
                  // Fetch the raw table data
                  this.apiService.getCustomers().subscribe({
                    next: (custRes) => {
                      this.loading = false;
                      this.customers = custRes.success && custRes.data ? custRes.data : [];
                      this.isDbEmpty = this.customers.length === 0;

                      if (!this.isDbEmpty) {
                        // Extract filter options
                        this.countriesList = Array.from(new Set(this.customers.map(c => c.country).filter(Boolean)));
                        this.educationList = Array.from(new Set(this.customers.map(c => c.education).filter(Boolean)));
                        
                        this.applyFilters();
                        this.generateInsights();
                        
                        setTimeout(() => {
                          this.renderCharts();
                        }, 0);
                      }
                    },
                    error: () => {
                      this.loading = false;
                      this.isDbEmpty = true;
                    }
                  });
                },
                error: () => {
                  this.loading = false;
                  this.isDbEmpty = true;
                }
              });
            },
            error: () => {
              this.loading = false;
              this.isDbEmpty = true;
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
      }
    });
  }

  applyFilters() {
    this.filteredCustomers = this.customers.filter(c => {
      const searchMatch = !this.searchText || 
                          `${c.firstName} ${c.lastName}`.toLowerCase().includes(this.searchText.toLowerCase()) ||
                          c.city.toLowerCase().includes(this.searchText.toLowerCase());
      const countryMatch = !this.selectedCountry || c.country === this.selectedCountry;
      const eduMatch = !this.selectedEducation || c.education === this.selectedEducation;
      const segmentMatch = !this.selectedSegment || c.rfmSegment === this.selectedSegment;

      return searchMatch && countryMatch && eduMatch && segmentMatch;
    });

    this.currentPage = 1;
    this.totalPages = Math.ceil(this.filteredCustomers.length / this.pageSize) || 1;
    this.updatePagination();
  }

  updatePagination() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    this.paginatedCustomers = this.filteredCustomers.slice(startIndex, startIndex + this.pageSize);
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

  generateInsights() {
    this.businessInsights = [];
    if (!this.summary || !this.analytics) return;

    const formatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

    // Insight 1: RFM High value contribution
    const highVal = this.summary.highValueCustomers;
    const totalSpend = this.summary.highValueCustomers.revenueContribution + 
                       this.summary.mediumValueCustomers.revenueContribution + 
                       this.summary.lowValueCustomers.revenueContribution;
    if (totalSpend > 0) {
      const highPct = Math.round((highVal.revenueContribution / totalSpend) * 100);
      this.businessInsights.push(`High-value customers represent ${Math.round(highVal.percentage * 100)}% of the buyer base but generate ${highPct}% of total platform revenue (${formatter.format(highVal.revenueContribution)}).`);
    }

    // Insight 2: Education vs Response
    const eduCompare = this.analytics.educationVsResponse;
    if (eduCompare && eduCompare.length > 0) {
      const sortedEdu = [...eduCompare].sort((a, b) => b.responseRate - a.responseRate);
      const topEdu = sortedEdu[0];
      this.businessInsights.push(`Customers with '${topEdu.category}' backgrounds show the highest campaign response rate at ${(topEdu.responseRate * 100).toFixed(1)}%.`);
    }

    // Insight 3: Age bracket spending
    const ageCompare = this.analytics.ageVsSpending;
    if (ageCompare && ageCompare.length > 0) {
      const sortedAge = [...ageCompare].sort((a, b) => b.averageSpend - a.averageSpend);
      const topAge = sortedAge[0];
      this.businessInsights.push(`Customers in the age bracket of '${topAge.category}' show the highest lifetime average spend at ${formatter.format(topAge.averageSpend)}.`);
    }

    // Insight 4: Lowest performing country response rate
    const countryCompare = this.analytics.countryVsResponse;
    if (countryCompare && countryCompare.length > 0) {
      const sortedCountry = [...countryCompare].sort((a, b) => a.responseRate - b.responseRate);
      const lowCountry = sortedCountry[0];
      this.businessInsights.push(`Campaign channels underperform in '${lowCountry.category}', showing the lowest overall response rate of ${(lowCountry.responseRate * 100).toFixed(1)}%.`);
    }

    // Insight 5: Repeat customer spend ratio
    if (this.summary.repeatPurchaseRate > 0) {
      const repeatCount = Math.round(this.summary.repeatPurchaseRate * this.summary.totalCustomers);
      const firstTimeSpend = this.summary.lowValueCustomers.averageSpend;
      const repeatSpend = this.summary.highValueCustomers.averageSpend;
      const ratio = firstTimeSpend > 0 ? (repeatSpend / firstTimeSpend).toFixed(1) : '2.0';
      this.businessInsights.push(`Repeat customers generate significantly higher value; high-value tier averages ${ratio}x higher spend per customer than the low-value tier.`);
    }
  }

  renderCharts() {
    if (!this.analytics) return;

    // Destroy existing charts to prevent memory leaks or overlay issues
    this.charts.forEach(c => c.destroy());
    this.charts = [];

    // 1. Age Distribution (Bar Chart)
    if (this.ageGenderCanvas) {
      const ctx = this.ageGenderCanvas.nativeElement.getContext('2d');
      const chart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.analytics.ageDistribution.map(x => x.range),
          datasets: [{
            label: 'Customer Count',
            data: this.analytics.ageDistribution.map(x => x.count),
            backgroundColor: '#0f2c59',
            borderRadius: 4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { display: false } }
        }
      });
      this.charts.push(chart);
    }

    // 2. Education Distribution (Pie Chart)
    if (this.eduCountryCanvas) {
      const ctx = this.eduCountryCanvas.nativeElement.getContext('2d');
      const chart = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: this.analytics.educationDistribution.map(x => x.range),
          datasets: [{
            data: this.analytics.educationDistribution.map(x => x.count),
            backgroundColor: ['#0f2c59', '#3b82f6', '#10b981', '#f59e0b', '#ec4899']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'bottom', labels: { boxWidth: 10, padding: 8 } } }
        }
      });
      this.charts.push(chart);
    }

    // 3. Income vs Spending (Bar Chart)
    if (this.incomeSpendingCanvas) {
      const ctx = this.incomeSpendingCanvas.nativeElement.getContext('2d');
      const chart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.analytics.incomeVsSpending.map(x => x.category),
          datasets: [{
            label: 'Avg Lifetime Spending ($)',
            data: this.analytics.incomeVsSpending.map(x => x.averageSpend),
            backgroundColor: '#2e7d32',
            borderRadius: 4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { ticks: { callback: (v) => '$' + v } } }
        }
      });
      this.charts.push(chart);
    }

    // 4. Age vs Spending (Line Chart)
    if (this.ageSpendingCanvas) {
      const ctx = this.ageSpendingCanvas.nativeElement.getContext('2d');
      const chart = new Chart(ctx, {
        type: 'line',
        data: {
          labels: this.analytics.ageVsSpending.map(x => x.category),
          datasets: [{
            label: 'Avg Lifetime Spending ($)',
            data: this.analytics.ageVsSpending.map(x => x.averageSpend),
            borderColor: '#3b82f6',
            backgroundColor: 'rgba(59, 130, 246, 0.1)',
            fill: true,
            tension: 0.2,
            borderWidth: 2
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { ticks: { callback: (v) => '$' + v } } }
        }
      });
      this.charts.push(chart);
    }

    // 5. Education vs Response (Bar Chart)
    if (this.eduResponseCanvas) {
      const ctx = this.eduResponseCanvas.nativeElement.getContext('2d');
      const chart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.analytics.educationVsResponse.map(x => x.category),
          datasets: [{
            label: 'Campaign Response Rate (%)',
            data: this.analytics.educationVsResponse.map(x => Math.round(x.responseRate * 100)),
            backgroundColor: '#ef4444',
            borderRadius: 4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { ticks: { callback: (v) => v + '%' } } }
        }
      });
      this.charts.push(chart);
    }

    // 6. Country vs Response (Bar Chart)
    if (this.countryResponseCanvas) {
      const ctx = this.countryResponseCanvas.nativeElement.getContext('2d');
      const chart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.analytics.countryVsResponse.map(x => x.category),
          datasets: [{
            label: 'Campaign Response Rate (%)',
            data: this.analytics.countryVsResponse.map(x => Math.round(x.responseRate * 100)),
            backgroundColor: '#f59e0b',
            borderRadius: 4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { ticks: { callback: (v) => v + '%' } } }
        }
      });
      this.charts.push(chart);
    }
  }
}
