# Marketing Campaign Analytics - User Guide

This user guide explains how to navigate, run, and execute the operational features of the platform.

---

## 1. Initial Setup: Loading and Seeding Data

Before accessing the dashboards, the database must contain data.

### Loading the Sample Kaggle Dataset
1. Open the Angular application and click **Upload Dataset** in the sidebar.
2. Under the **Sample Dataset** card, click **Load Default Kaggle Dataset**.
3. A spinner overlay will appear while SQL Server seeds. Once complete, a success toast will show, and the system will automatically train the prediction machine learning model.

### Uploading a Custom CSV
1. Go to the **Upload Dataset** page.
2. Under **Custom Dataset Upload**, select a local CSV file. The file must contain standard columns such as `Year_Birth`, `Income`, `Education`, and campaign metrics (e.g. `MntWines`, `NumWebPurchases`, `Response`).
3. Click **Upload and Seed**. The database will wipe existing entries, load the new records, re-seed campaign details, and train a new Logistic Regression classifier.

---

## 2. Navigating Dashboards

- **Dashboard**: Displays main KPIs (Total Spend, Total Revenue, Conversion rates), monthly trend charts, best performing campaigns, and channel performance metrics.
- **Campaign Performance**: Inspect detailed campaigns in a searchable table. Apply filters (Channel, Type, Status) or search by name. Dynamic charts (Spend vs Revenue, Channel Revenue share, ROI timeline) adjust to your filters.
- **Campaign Comparison**: Select any two campaigns from the dropdown menus to display side-by-side metrics. Winning parameters (e.g. higher ROI or conversions) are highlighted.
- **Campaign Effectiveness**: Displays campaign ROI margin badges (Excellent, Good, Average, Needs Improvement) and status colors to quickly review marketing assets.
- **Customer Insights**: Shows demographic distributions (Age, Income, Gender, Education, Country) in a vertical layout. Explores RFM Value segments, customer personas, behavior metrics, dynamic business insights, and a searchable Customer Directory table.

---

## 3. Generating Excel & PDF Reports

1. Click **Reports** in the sidebar.
2. Click **Generate Excel Report** to run Python's `openpyxl` script. The file downloads immediately, and is styled with freeze panes, negative-ROI highlights, and native charts.
3. Click **Generate PDF Report** to run Python's `reportlab` script. It downloads a PDF detailing campaign KPI tables, customer segments, and 5-8 automated business recommendations.
4. Review the **Recently Generated Reports** grid to re-download previously archived documents.

---

## 4. Running Campaign Response Predictions

1. Click **Response Prediction** in the sidebar.
2. The bottom section displays the **Model Metrics** (Accuracy, Precision, F1 Score, ROC AUC, Confusion Matrix, and Classification Report text) loaded from `model_metrics.json`.
3. In the prediction form, enter a customer profile (Age, Income, Education level, Total Purchases, Average Spend per transaction, and the Campaign channel to target).
4. Click **Predict Likelihood**.
5. The result card displays a Verdict Badge (Likely/Not Likely), Probability progress bar, Confidence tier (High, Medium, Low), and 3-5 rule-based business explanations detailing why the model returned that outcome.

---

## 5. Tableau BI Dataset Exports

1. Click **Business Intelligence** in the sidebar.
2. Click **Export All** to trigger C# database exports. The 4 CSV tables (`campaign_performance.csv`, `customer_analytics.csv`, `campaign_summary.csv`, `monthly_revenue.csv`) will download to your local machine and save on the server under `/Tableau/Datasets`.
3. Open **Tableau**, connect to these CSV files, and build the executive dashboards described in the BI documentation.
