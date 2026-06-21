# Tableau Dashboards Documentation

This document describes the structure, data modeling, and creation steps for the four professional Tableau dashboards designed for the Marketing Campaign Analytics Platform.

---

## Data Source Connection Guide

1. Export all clean datasets from the Angular interface or via `/api/tableau/export/all`.
2. Open **Tableau Desktop** or **Tableau Public**.
3. Under **Connect -> To a File**, select **Text File** and load `campaign_performance.csv`.
4. Add connections for `customer_analytics.csv`, `campaign_summary.csv`, and `monthly_revenue.csv`.

### Data Model Relations
For advanced dashboards, join the tables on the logical keys:
- **Relation**: `customer_analytics.csv` joined to `campaign_performance.csv` (many-to-many through mapping if needed, or analyze independently based on exported granularity).
- `campaign_performance.csv` and `monthly_revenue.csv` can be joined on the Date/Month dimensions.

---

## Dashboard 1: Executive Dashboard
**Objective**: A high-level overview of marketing investments, total revenue, ROI margins, and conversions over time for the leadership team.

### Layout & Elements
- **KPI Summary Cards (Top)**:
  - **Total Revenue**: `SUM(Revenue)` from `campaign_summary.csv` or `campaign_performance.csv`.
  - **Total Spend**: `SUM(Spend)`.
  - **Return on Investment (ROI)**: Calculated field: `(SUM(Revenue) - SUM(Spend)) / SUM(Spend)`.
  - **Conversions**: `SUM(Conversions)`.
- **Revenue & Spend Trend (Middle)**:
  - Dual axis chart with **Revenue** (Bar) and **Campaign Spend** (Area) plotted against **Month** (`monthly_revenue.csv`).
- **Revenue by Channel Contribution (Bottom)**:
  - **Pie Chart** illustrating the distribution of total revenue across marketing channels (Email, SMS, Social, Google Search, Display).

---

## Dashboard 2: Campaign Performance Dashboard
**Objective**: Detailed comparison of individual campaigns to measure ROAS, ROI, and budget utilization.

### Layout & Elements
- **Spend vs Revenue comparison**:
  - Horizontal side-by-side bar chart showing **Spend** and **Revenue** for each campaign name.
- **ROI Scatter Plot**:
  - **ROI** (Y-Axis) vs. **Spend** (X-Axis) with color encoding by **Marketing Channel**. This highlights campaigns with high returns relative to their spend.
- **Marketing Channel Performance Grid**:
  - A matrix table displaying **Conversions**, **Average ROAS**, **CTR**, and **Response Rate** grouped by marketing channels.

---

## Dashboard 3: Customer Analytics Dashboard
**Objective**: Interactive demographics explorer detailing segments, personas, income distribution, and purchasing behaviours.

### Layout & Elements
- **RFM Customer Segments**:
  - **Treemap** visualization showing customer counts grouped by RFM Segment (High Value, Medium Value, Low Value).
- **Age and Income Distributions**:
  - **Histogram** of customer ages (binned into 10-year groups) alongside a **Scatter Plot** mapping **Income** vs. **Average Spend**.
- **Customer Personas Matrix**:
  - Card layouts detailing average income, spending, and conversion rate per standard persona (High Value, Frequent Buyers, Occasional Buyers, At Risk).

---

## Dashboard 4: Marketing Insights Dashboard
**Objective**: Analytical storytelling visualizer highlighting effectiveness ratings, response rates, and business impact recommendations.

### Layout & Elements
- **Campaign Effectiveness Rating**:
  - Heatmap grid sorting campaigns by ROI, color-coded by performance tiers:
    - *Excellent* (ROI $\ge$ 60%)
    - *Good* (ROI $\ge$ 30%)
    - *Average* (ROI $\ge$ 0%)
    - *Needs Improvement* (ROI $<$ 0%)
- **Outreach Channel Response Rates**:
  - **Funnel Chart** showing campaign impressions -> clicks -> conversions -> campaign responses.
- **Executive Business Recommendations List**:
  - Text-container displaying rule-based strategic actions (e.g. reallocating capital, campaign optimization) driven by data analysis.
