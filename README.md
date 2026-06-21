# Marketing Campaign Analytics Platform

An enterprise-grade, full-stack marketing measurement and customer value analytics platform. The application connects an Angular client dashboard, an ASP.NET Core Web API, a SQL Server database, and a live Python process execution engine to analyze campaign ROI, customer value segmentation, statistical significance, and real-time response prediction.

---

## 1. Project Overview

Marketing teams frequently spend millions of dollars on campaigns across multiple channels (Email, SMS, Social, Search, Display) without precise clarity on which campaigns are driving true incremental conversions.

This platform bridges that gap by providing:
- **Executive KPI Tracking**: High-level financial metrics (Revenue, Spend, ROI, Conversion Rates).
- **Campaign Performance & Comparison**: Detailed channel breakdowns and effectiveness rankings.
- **Customer Insights**: RFM segmentation dividing customers into High, Medium, and Low Value tiers.
- **Data Analysis & Statistics**: Exploratory Data Analysis (EDA) and automated statistical hypothesis testing (T-Test, Chi-Square, Linear Regression) with natural language business explanations.
- **Predictive Analytics**: Real-time response forecasting powered by a Python-backed Logistic Regression classifier.
- **Document Automation**: Dynamically generated, formatted Excel workbooks (openpyxl) and PDF briefs (reportlab).

---

## 2. Technology Stack

- **Frontend**: Angular 19, TypeScript, Bootstrap 5, Chart.js.
- **Backend API**: ASP.NET Core Web API (.NET 8), Entity Framework Core (EF Core).
- **Database**: SQL Server Express LocalDB (`MSSQLLocalDB`).
- **Analytics Engine**: Python (Pandas, NumPy, Scikit-Learn, SciPy, StatsModels).
- **Document Automation**: openpyxl (Excel), ReportLab (PDF).
- **BI Layer**: Tableau Desktop (`.twb` templates).

---

## 3. Database Schema

The database utilizes SQL Server LocalDB and maps relational entities:

### 1. `Customer`
- `CustomerId` (INT, Primary Key, Identity)
- `Age` (INT)
- `Gender` (NVARCHAR)
- `Income` (DECIMAL)
- `Education` (NVARCHAR)
- `MaritalStatus` (NVARCHAR)
- `Country` (NVARCHAR)

### 2. `Campaign`
- `CampaignId` (INT, Primary Key, Identity)
- `CampaignName` (NVARCHAR)
- `Channel` (NVARCHAR)
- `Budget` (DECIMAL)
- `Spend` (DECIMAL)
- `Revenue` (DECIMAL)
- `Conversions` (INT)
- `Clicks` (INT)
- `Impressions` (INT)
- `StartDate` (DATETIME)
- `EndDate` (DATETIME)

### 3. `CampaignResponse`
- `ResponseId` (INT, Primary Key, Identity)
- `CustomerId` (INT, Foreign Key referencing Customer)
- `CampaignId` (INT, Foreign Key referencing Campaign)
- `Response` (NVARCHAR)
- `PurchaseAmount` (DECIMAL)
- `PurchaseDate` (DATETIME)

---

## 4. Application Workflow

```text
       [ Angular Client ]  ◄── (HTTP/CORS) ──►  [ ASP.NET Core Web API ]
               │                                       │
               ▼                                       ▼
     [ Interactive Charts ]                     [ SQL Server DB ]
     [ Forms & Predictors ]                            │ (Process Runner)
                                                       ▼
                                            [ Python Analytics Engine ]
                                            (eda, stats, ml, reports)
```

1. **User Action**: The user launches the Angular frontend. On first load, they can choose to load the pre-cleaned Kaggle *Customer Personality Analysis* dataset or upload their own custom CSV.
2. **Data Aggregation**: ASP.NET Core uses Entity Framework Core and LINQ to run aggregated KPI, performance, and RFM segmentation queries directly against SQL Server.
3. **Deep Analytics**: When the user requests statistical tests, EDA matrices, or ML models, ASP.NET Core spawns a Python subprocess, passes parameters, and captures the JSON stdout payload.
4. **Document Export**: Downloading reports triggers a Python background process that writes clean Excel sheets (featuring conditional formatting, pivot summaries, and native charts) or styled PDFs.

---

## 5. Directory Structure

```text
MarketingCampaignAnalytics/
├── Frontend/                         # Angular Client
│   ├── src/app/
│   │   ├── components/               # All page layouts
│   │   ├── shared/                   # Sidebar, Navbar, Footer
│   │   ├── services/                 # ApiService
│   │   └── models/                   # TypeScript interfaces
├── Backend/                          # ASP.NET Core Web API
│   ├── Controllers/                  # Endpoints (Upload, Dashboard, Reports, Analytics)
│   ├── Models/                       # Database Entity Classes
│   ├── DTOs/                         # Request/Response Data structures
│   ├── Database/                     # DbContext, DbInitializer
│   ├── Services/                     # Business services & PythonRunner
│   ├── Middleware/                   # GlobalExceptionMiddleware
│   └── Analytics/                    # Python core scripts
├── Tableau/                          # Dedicated Tableau worksheets (.twb)
├── Reports/                          # Generated download directory
└── README.md                         # Project documentation
```

---

## 6. How to Run (Local Setup)

### Prerequisites
- .NET 8 SDK
- Node.js (v20+) & Angular CLI (v17+)
- Python (v3.10+) with pip
- SQL Server Express LocalDB

### 1. Backend Setup
1. Open PowerShell and navigate to `Backend/`.
2. Run `dotnet restore` to restore packages.
3. Start the API server:
   ```bash
   dotnet run
   ```
4. Confirm Swagger is running at `http://localhost:5246/swagger`.

### 2. Frontend Setup
1. Navigate to `Frontend/`.
2. Install npm dependencies:
   ```bash
   npm install
   ```
3. Run the development server:
   ```bash
   npm start
   ```
4. Open the application at `http://localhost:4200`.
