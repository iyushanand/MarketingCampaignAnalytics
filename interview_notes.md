# Technical Interview Preparation Guide

This document prepares candidates for technical interviews (e.g., American Express AIM Analyst, Big 4 consulting firms, Senior BI/Full Stack roles) using the architecture, algorithms, and design choices of the **Marketing Campaign Analytics Platform**.

---

## 1. Domain Knowledge & Core Marketing Metrics

Interviewers will test your understanding of business metrics and how they translate to code:

*   **Return on Investment (ROI)**:
    $$\text{ROI} = \frac{\text{Revenue} - \text{Spend}}{\text{Spend}} \times 100$$
    *Code implementation*: In `ReportService.cs`, this is calculated as `(double)(totalRevenue - totalSpend) / (double)totalSpend * 100.0`.
*   **Return on Ad Spend (ROAS)**:
    $$\text{ROAS} = \frac{\text{Revenue}}{\text{Spend}}$$
    *Key difference*: ROI includes net profitability (accounting for spend as cost), while ROAS is a simple multiple showing top-line revenue generated per dollar spent.
*   **Click-Through Rate (CTR)**:
    $$\text{CTR} = \frac{\text{Clicks}}{\text{Impressions}}$$
*   **Response Rate**:
    $$\text{Response Rate} = \frac{\text{Number of Positive Responses ("Yes")}}{\text{Total Campaign Impressions / Contacts}}$$
*   **RFM Segmentation**:
    *   **Recency**: How recently a customer purchased.
    *   **Frequency**: How often they purchase.
    *   **Monetary**: How much they spend.
    *   *Our Implementation*: Customers are classified into:
        *   **High Value**: Total spend $\ge$ 1.2 $\times$ Average Customer Spend.
        *   **Low Value**: Total spend $<$ 0.5 $\times$ Average Customer Spend.
        *   **Medium Value**: Customers falling in between.

---

## 2. System Architecture & Inter-Process Communication

*Question: "How does a C# ASP.NET Core service execute Python code and retrieve the results?"*

*   **Process Spawning**: We use C#'s `System.Diagnostics.Process` class to execute a Python subprocess.
*   **Data Serialization**:
    1.  C# serializes relational data from SQL Server into a JSON temporary file (using `System.Text.Json`).
    2.  The file path is passed as a command-line argument to the Python script.
    3.  Python reads the JSON file, runs calculations (Pandas, SciPy, Scikit-Learn), and prints the results as a JSON string to `stdout`.
    4.  C# captures the standard output stream, deletes the temp file, and deserializes the JSON string into strongly-typed DTOs.
*   **Performance Optimization**:
    *   *Challenge*: Spawning processes is computationally heavy.
    *   *Solution*: For standard dashboard queries, we utilize EF Core and LINQ to perform all aggregations directly in SQL Server. We only spawn the Python subprocess for complex statistical testing, machine learning predictions, or document rendering.

---

## 3. Machine Learning (Logistic Regression) & Predictions

*Question: "Why did you choose Logistic Regression for Campaign Response Prediction?"*

*   **Interpretability**: Unlike "black box" models (like XGBoost or Neural Networks), Logistic Regression provides clear coefficients. In marketing, understanding *why* a customer responds (e.g., income coefficient, channel effect) is as important as the prediction itself.
*   **Pipeline Details**:
    1.  **Categorical Encoding**: One-Hot Encoding is applied to categorical features (Education, Marketing Channel).
    2.  **Feature Scaling**: Numerical features (Age, Income, Spend) are scaled using `StandardScaler` to prevent larger scale values from dominating.
    3.  **Model Metrics**: The pipeline evaluates model quality using:
        *   **Accuracy**: Overall correct rate.
        *   **Precision**: $\frac{TP}{TP + FP}$ (Minimizes false positives - vital if campaign contact costs are high).
        *   **Recall**: $\frac{TP}{TP + FN}$ (Minimizes missed opportunities).
        *   **ROC-AUC**: Evaluates classification threshold performance independent of target imbalance.

---

## 4. SQL & Database Performance Optimization

*Question: "How did you optimize Entity Framework Core queries for performance?"*

*   **N+1 Query Resolution**:
    *   *Problem*: Executing a database query inside an in-memory loop (e.g., fetching campaign response counts for every single campaign row).
    *   *Solution*: Grouping responses inside the database using LINQ `GroupBy()` and executing a single aggregated query, loading the results into a C# dictionary for fast $O(1)$ lookups:
        ```csharp
        var responseStats = await _context.CampaignResponses
            .GroupBy(r => r.CampaignId)
            .Select(g => new {
                CampaignId = g.Key,
                TotalResponses = g.Count(),
                YesResponses = g.Count(r => r.Response == "Yes")
            })
            .ToDictionaryAsync(x => x.CampaignId, x => new { x.TotalResponses, x.YesResponses });
        ```
*   **Indexes**: Essential indexes were mapped on Foreign Keys (`CustomerId`, `CampaignId`) in the `CampaignResponse` table, ensuring join and group queries resolve in sub-millisecond times.

---

## 5. Reporting and Data Visualization (Tableau)

*Question: "How did you design the Tableau integration layer?"*

*   **Star Schema Export**: The platform exports flat datasets modeling a star schema (fact and dimension files: `campaign_performance.csv`, `customer_analytics.csv`, etc.).
*   **Tableau Relations**:
    *   Join datasets on logical keys (e.g., `CustomerId` or `CampaignId`).
    *   Using logical relations in Tableau rather than physical joins allows Tableau to handle aggregations at different levels of granularity without duplicating data.
