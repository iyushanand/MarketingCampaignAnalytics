# Marketing Campaign Analytics Platform

An enterprise-grade, full-stack marketing measurement, customer segmentation, and predictive targeting system. This platform enables marketing teams and business executives to optimize ad spend, measure campaign ROI, group customers by purchase behavior, perform advanced statistical testing, and predict target response likelihoods.

Designed as a professional portfolio showcase for analytical and full-stack engineering roles (e.g., **American Express AIM Analyst**, **Deloitte**, **EY**, **Accenture**), it demonstrates software engineering best practices, database performance tuning, inter-process communication, and machine learning pipeline integration.

---

## 🚀 Business Value & Key Capabilities

1.  **Executive Financial Intelligence**: Real-time tracking of critical KPIs including **Total Revenue**, **Total Spend**, **ROI Margins**, and **Conversion Rates** with visual trends over time.
2.  **Multi-Channel Attribution**: Deep-dive analytics on marketing channels (Email, SMS, Social Media, Google Search, Display) to identify high-performing campaigns and optimize budget allocation.
3.  **Customer RFM Segmentation**: In-memory grouping of customers into **High Value**, **Medium Value**, and **Low Value** tiers based on Recency, Frequency, and Monetary spend profiles.
4.  **Automated Hypothesis Testing**: Advanced Python-driven statistics (Welch's T-Test, Chi-Square of Independence, and Ordinary Least Squares Linear Regression) with natural language translations of p-values for business users.
5.  **Predictive targeting (ML)**: A machine learning pipeline utilizing a **scikit-learn Logistic Regression Classifier** to predict campaign response likelihood, complete with classification reports, confusion matrices, and ROC-AUC metrics.
6.  **Document Automation**: Scheduled and on-demand styled Excel workbooks (featuring freeze-panes, zebra striping, conditional highlights, and native charts via `openpyxl`) and paginated executive PDF briefs (via `reportlab`).
7.  **Tableau BI Integration**: Automatic star-schema CSV dataset exports to feed enterprise Tableau dashboards.

---

## 🛠️ Technology Stack

*   **Frontend**: Angular 19 (Standalone Components, Routing, CORS-ready client), TypeScript, Bootstrap 5, Chart.js.
*   **Backend API**: ASP.NET Core Web API (.NET 8), Entity Framework Core (EF Core), LINQ.
*   **Database**: SQL Server LocalDB (`MSSQLLocalDB`).
*   **Analytics Engine**: Python (Pandas, NumPy, Scikit-Learn, SciPy, StatsModels).
*   **Document Automation**: openpyxl (Excel), ReportLab (PDF).
*   **BI Layer**: Tableau Public / Tableau Desktop (`.csv` export pipeline).

---

## 📐 Architecture & System Flow

```text
       [ Angular 19 Client ]
                │
                ▼ (CORS HTTP REST API)
       [ ASP.NET Core Web API ] ◄──► [ SQL Server Relational DB ]
                │
                ▼ (Asynchronous Python Process Execution)
       [ Python Analytics Hub ]
        ├── eda.py (Data profiling & correlation matrices)
        ├── statistics.py (T-test, Chi-square, Linear regression)
        ├── machine_learning.py (Logistic Regression fit & predict)
        └── report_generator.py (Styled Excel & PDF rendering)
```

*   **Process Isolation**: ASP.NET Core manages data transactions and seeds. For computationally heavy analytical logic (ML fitting, PDFs, stats), the API spawns an isolated Python subprocess, executing scripts asynchronously, passing payload filepaths, and returning structured JSON stdout.
*   **Performance Tuning**: Resolved classic **N+1 query issues** in Entity Framework Core by utilizing SQL aggregations and grouped memory mapping, reducing DB load from $O(N)$ query loops to $O(1)$ dictionary lookups.

For deep details, review our separate documentation:
*   📄 [architecture.md](file:///C:/Users/KIIT/.gemini/antigravity/scratch/MarketingCampaignAnalytics/architecture.md) - Layer configurations and execution workflow diagrams.
*   📄 [database_design.md](file:///C:/Users/KIIT/.gemini/antigravity/scratch/MarketingCampaignAnalytics/database_design.md) - Entity Relationship Diagram (ERD) and relational schema mappings.
*   📄 [api_documentation.md](file:///C:/Users/KIIT/.gemini/antigravity/scratch/MarketingCampaignAnalytics/api_documentation.md) - Swagger routes, JSON payloads, and response definitions.
*   📄 [user_guide.md](file:///C:/Users/KIIT/.gemini/antigravity/scratch/MarketingCampaignAnalytics/user_guide.md) - Instructions for seeding, predicting, and reporting.

---

## 📦 Directory Structure

```text
MarketingCampaignAnalytics/
├── Frontend/                         # Angular 19 Client App
│   ├── src/app/
│   │   ├── components/               # Dashboards, ML Target Form, Reports, BI Tabs
│   │   ├── services/                 # ApiService (HTTP Client)
│   │   └── models/                   # TypeScript interfaces & DTO mappings
├── Backend/                          # ASP.NET Core Web API (.NET 8)
│   ├── Controllers/                  # Endpoints (Reports, Tableau, Uploads, Analytics)
│   ├── Database/                     # DbContext and DbInitializer (seeding Kaggle datasets)
│   ├── Services/                     # C# Services (TableauExport, Reports, Predictions, PythonRunner)
│   └── Analytics/                    # Python analytical execution scripts
├── Tableau/                          # Dedicated Tableau Integration
│   ├── Datasets/                     # Exported CSV star-schema data tables
│   └── Documentation/                # Tableau data connection guides & relations
├── docs/                             # Project assets
│   └── screenshots/                  # Beautiful application screenshots
└── README.md                         # Project Case Study
```

---

## ⚙️ How to Run Locally

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js (v20+)](https://nodejs.org/) & Angular CLI
*   [Python 3.10+](https://www.python.org/downloads/) with pip
*   [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)

### 1. Backend API Configuration
1.  Navigate to the `Backend` directory:
    ```powershell
    cd Backend
    ```
2.  Install Python packages:
    ```powershell
    pip install pandas numpy scikit-learn scipy statsmodels openpyxl reportlab joblib
    ```
3.  Ensure database connection strings and the python execution path match your local setup in `appsettings.json`.
4.  Build and run the API server:
    ```powershell
    dotnet run
    ```
5.  Open [http://localhost:5224/swagger](http://localhost:5224/swagger) in your browser to verify Swagger endpoint documentation.

### 2. Frontend Angular App
1.  Navigate to the `Frontend` directory:
    ```powershell
    cd Frontend
    ```
2.  Install all node modules:
    ```powershell
    npm install
    ```
3.  Launch the development server:
    ```powershell
    npm start
    ```
4.  Access the platform dashboard at [http://localhost:4200](http://localhost:4200).

---

## 📊 Key Highlights for Recruiting Managers

*   **American Express AIM Role Alignment**: Features industry-standard RFM modeling, campaign ROI measurement, and response scoring. Showcases an understanding of budget-optimal targeting (Logistic Regression coefficients and precision-oriented threshold tuning).
*   **Advanced ML Pipeline**: The Logistic Regression classifier runs custom categorical One-Hot Encoding and Standard Scaling. Model coefficients are evaluated to provide business-level rationales (e.g., "Customer average spend is above the market average, increasing response probability").
*   **Production-Grade Stability**: Includes a global API exception handling middleware, explicit database transactional constraints, and asynchronous thread-safe process managers preventing memory leakage.
*   **Zero-Warning Build Quality**: Code compiles with zero warnings or type compiler flags on both .NET 8 and Angular 19 platforms.
