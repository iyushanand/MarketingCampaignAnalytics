import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  navItems = [
    { label: 'Dashboard', path: '/dashboard', icon: 'bi bi-grid-1x2-fill' },
    { label: 'Campaign Performance', path: '/campaign-performance', icon: 'bi bi-speedometer2' },
    { label: 'Campaign Comparison', path: '/campaign-comparison', icon: 'bi bi-arrow-left-right' },
    { label: 'Campaign Effectiveness', path: '/campaign-effectiveness', icon: 'bi bi-shield-check' },
    { label: 'Customer Insights', path: '/customer-insights', icon: 'bi bi-people-fill' },
    { label: 'Data Analysis', path: '/data-analysis', icon: 'bi bi-activity' },
    { label: 'Marketing Reports', path: '/marketing-reports', icon: 'bi bi-file-earmark-bar-graph' },
    { label: 'Statistics', path: '/statistics', icon: 'bi bi-calculator-fill' },
    { label: 'Response Prediction', path: '/response-prediction', icon: 'bi bi-magic' },
    { label: 'ROI Calculator', path: '/roi-calculator', icon: 'bi bi-cash-coin' },
    { label: 'Reports', path: '/reports', icon: 'bi bi-download' },
    { label: 'Business Intelligence', path: '/business-intelligence', icon: 'bi bi-bar-chart-line-fill' },
    { label: 'Upload Dataset', path: '/upload-dataset', icon: 'bi bi-cloud-arrow-up-fill' }
  ];
}
