import sys
import os
import json
import warnings
import pandas as pd
import numpy as np

# Suppress all pandas/numpy runtime and deprecation warnings
warnings.filterwarnings("ignore")

def run_eda(csv_path):
    """
    Executes Exploratory Data Analysis (EDA) on the input CSV file.
    Returns a dictionary of analysis metrics or error details.
    """
    if not os.path.exists(csv_path):
        return {
            "success": False,
            "error": f"File not found: {csv_path}"
        }
    
    try:
        # Detect delimiter (comma or tab)
        with open(csv_path, 'r', encoding='utf-8', errors='ignore') as f:
            first_line = f.readline()
        
        separator = '\t' if '\t' in first_line else ','
        
        df = pd.read_csv(csv_path, sep=separator)
    except Exception as e:
        return {
            "success": False,
            "error": f"Failed to read CSV file: {str(e)}"
        }
    
    # Validate required demographic columns (minimum required to perform analysis)
    required_cols = ["Year_Birth", "Income", "Education", "Marital_Status"]
    missing_req = [col for col in required_cols if col not in df.columns]
    if missing_req:
        return {
            "success": False,
            "error": f"Missing required columns: {', '.join(missing_req)}"
        }
    
    # 1. Dataset Summary
    num_rows = int(df.shape[0])
    num_cols = int(df.shape[1])
    col_names = list(df.columns)
    dtypes = {col: str(dtype) for col, dtype in df.dtypes.items()}
    memory_usage = int(df.memory_usage(deep=True).sum())
    
    # 2. Missing Value Analysis
    missing_analysis = {}
    for col in df.columns:
        m_count = int(df[col].isna().sum())
        m_pct = float((m_count / num_rows) * 100) if num_rows > 0 else 0.0
        missing_analysis[col] = {
            "missing_count": m_count,
            "missing_percentage": round(m_pct, 4),
            "is_high_missing": m_pct > 20.0
        }
        
    # 3. Duplicate Analysis
    num_duplicates = int(df.duplicated().sum())
    duplicate_percentage = float((num_duplicates / num_rows) * 100) if num_rows > 0 else 0.0
    
    # Exclude identifiers and zero-variance/constant columns from statistics & correlation
    cols_to_exclude = ["ID", "Z_CostContact", "Z_Revenue"]
    numeric_df = df.select_dtypes(include=[np.number])
    numeric_cols = [c for c in numeric_df.columns if c not in cols_to_exclude]
    
    # 4. Summary Statistics
    summary_stats = {}
    for col in numeric_cols:
        col_series = df[col].dropna()
        if len(col_series) == 0:
            continue
        summary_stats[col] = {
            "mean": float(col_series.mean()),
            "median": float(col_series.median()),
            "std": float(col_series.std()) if len(col_series) > 1 else 0.0,
            "min": float(col_series.min()),
            "max": float(col_series.max()),
            "q25": float(col_series.quantile(0.25)),
            "q50": float(col_series.quantile(0.50)),
            "q75": float(col_series.quantile(0.75))
        }
        
    # 5. Outlier Detection (IQR Method)
    outliers_info = {}
    for col in numeric_cols:
        col_series = df[col].dropna()
        if len(col_series) == 0:
            outliers_info[col] = {"outlier_count": 0, "outlier_percentage": 0.0}
            continue
        q25 = col_series.quantile(0.25)
        q75 = col_series.quantile(0.75)
        iqr = q75 - q25
        lower_bound = q25 - 1.5 * iqr
        upper_bound = q75 + 1.5 * iqr
        
        outlier_count = int(((col_series < lower_bound) | (col_series > upper_bound)).sum())
        outlier_pct = float((outlier_count / len(col_series)) * 100) if len(col_series) > 0 else 0.0
        
        outliers_info[col] = {
            "outlier_count": outlier_count,
            "outlier_percentage": round(outlier_pct, 4)
        }
        
    # 6. Correlation Analysis (Pearson)
    corr_matrix_raw = df[numeric_cols].corr().fillna(0.0)
    correlation_matrix = {}
    for col1 in corr_matrix_raw.index:
        correlation_matrix[col1] = {}
        for col2 in corr_matrix_raw.columns:
            correlation_matrix[col1][col2] = float(corr_matrix_raw.loc[col1, col2])
            
    strong_positive = []
    strong_negative = []
    
    cols_corr = list(corr_matrix_raw.columns)
    for idx1 in range(len(cols_corr)):
        for idx2 in range(idx1 + 1, len(cols_corr)):
            col1 = cols_corr[idx1]
            col2 = cols_corr[idx2]
            coef = float(corr_matrix_raw.loc[col1, col2])
            if coef > 0.7:
                strong_positive.append({
                    "feature1": col1,
                    "feature2": col2,
                    "coefficient": round(coef, 4)
                })
            elif coef < -0.7:
                strong_negative.append({
                    "feature1": col1,
                    "feature2": col2,
                    "coefficient": round(coef, 4)
                })
                
    # 7. Customer Behaviour Insights
    avg_income = float(df['Income'].mean()) if 'Income' in df.columns else 0.0
    
    # Total spending per customer
    spending_cols = ['MntWines', 'MntFruits', 'MntMeatProducts', 'MntFishProducts', 'MntSweetProducts', 'MntGoldProds']
    available_spend_cols = [c for c in spending_cols if c in df.columns]
    if available_spend_cols:
        df['TotalSpent'] = df[available_spend_cols].sum(axis=1)
        avg_purchase_amount = float(df['TotalSpent'].mean())
    else:
        df['TotalSpent'] = 0.0
        avg_purchase_amount = 0.0
        
    # Total purchases per customer
    purchase_cols = ['NumWebPurchases', 'NumCatalogPurchases', 'NumStorePurchases']
    available_purchase_cols = [c for c in purchase_cols if c in df.columns]
    if available_purchase_cols:
        df['TotalPurchases'] = df[available_purchase_cols].sum(axis=1)
        avg_number_of_purchases = float(df['TotalPurchases'].mean())
    else:
        df['TotalPurchases'] = 0.0
        avg_number_of_purchases = 0.0
        
    most_common_edu = str(df['Education'].mode()[0]) if 'Education' in df.columns and len(df['Education'].dropna()) > 0 else "N/A"
    most_common_marital = str(df['Marital_Status'].mode()[0]) if 'Marital_Status' in df.columns and len(df['Marital_Status'].dropna()) > 0 else "N/A"
    
    top_spend_category = "N/A"
    if available_spend_cols:
        category_sums = df[available_spend_cols].sum()
        prettify_map = {
            'MntWines': 'Wines',
            'MntFruits': 'Fruits',
            'MntMeatProducts': 'Meat',
            'MntFishProducts': 'Fish',
            'MntSweetProducts': 'Sweets',
            'MntGoldProds': 'Gold'
        }
        top_spend_col = category_sums.idxmax()
        top_spend_category = prettify_map.get(top_spend_col, top_spend_col)
        
    # Campaign metadata mapping consistent with ASP.NET Core DB initializer
    campaigns_info = [
        {"name": "Campaign 1", "channel": "Email", "col": "AcceptedCmp1", "spend": 48000.0, "budget": 50000.0},
        {"name": "Campaign 2", "channel": "SMS", "col": "AcceptedCmp2", "spend": 28000.0, "budget": 30000.0},
        {"name": "Campaign 3", "channel": "Social Media", "col": "AcceptedCmp3", "spend": 58000.0, "budget": 60000.0},
        {"name": "Campaign 4", "channel": "Google Search Ads", "col": "AcceptedCmp4", "spend": 39000.0, "budget": 40000.0},
        {"name": "Campaign 5", "channel": "Display Ads", "col": "AcceptedCmp5", "spend": 44000.0, "budget": 45000.0},
        {"name": "Campaign 6", "channel": "Email", "col": "Response", "spend": 78000.0, "budget": 80000.0}
    ]
    
    campaigns_results = []
    channel_revenues = {}
    
    for c in campaigns_info:
        col = c["col"]
        if col in df.columns:
            responses = int(df[col].sum())
            resp_rate = float(df[col].mean())
            # Revenue calculation uses the 1/3 spending allocation model from the C# seeder
            revenue = float(df[df[col] == 1]['TotalSpent'].sum() / 3.0)
        else:
            responses = 0
            resp_rate = 0.0
            revenue = 0.0
            
        roi = (revenue - c["spend"]) / c["spend"] if c["spend"] > 0 else 0.0
        
        c_res = {
            "name": c["name"],
            "channel": c["channel"],
            "col": col,
            "responses": responses,
            "response_rate": round(resp_rate, 4),
            "spend": c["spend"],
            "budget": c["budget"],
            "revenue": round(revenue, 2),
            "roi": round(roi, 4)
        }
        campaigns_results.append(c_res)
        
        channel = c["channel"]
        channel_revenues[channel] = channel_revenues.get(channel, 0.0) + revenue
        
    highest_rev_channel = "N/A"
    if channel_revenues:
        highest_rev_channel = max(channel_revenues, key=channel_revenues.get)
        
    # 8. Campaign Insights
    total_campaigns = len(campaigns_info)
    total_responses = sum(c["responses"] for c in campaigns_results)
    
    total_opportunities = total_campaigns * num_rows
    overall_response_rate = float(total_responses / total_opportunities) if total_opportunities > 0 else 0.0
    
    avg_revenue = float(np.mean([c["revenue"] for c in campaigns_results]))
    avg_spend = float(np.mean([c["spend"] for c in campaigns_results]))
    
    best_campaign = "N/A"
    worst_campaign = "N/A"
    if campaigns_results:
        best_campaign = max(campaigns_results, key=lambda x: x["roi"])["name"]
        worst_campaign = min(campaigns_results, key=lambda x: x["roi"])["name"]

    # 9. Business Insights
    insights = []
    
    # Category spending
    insights.append(f"Product category '{top_spend_category}' accounts for the highest customer spend.")
    # Channels
    insights.append(f"Marketing channels grouped under '{highest_rev_channel}' generated the highest calculated revenue.")
    
    # Income spending correlation
    if 'Income' in df.columns and 'TotalSpent' in df.columns:
        corr_income_spend = df['Income'].corr(df['TotalSpent'])
        if pd.notna(corr_income_spend):
            if corr_income_spend > 0.5:
                insights.append(f"Higher customer income exhibits a strong positive correlation ({round(corr_income_spend, 2)}) with higher total spending.")
            elif corr_income_spend > 0.2:
                insights.append(f"Higher customer income exhibits a moderate positive correlation ({round(corr_income_spend, 2)}) with higher total spending.")
            else:
                insights.append("Income has a weak correlation with campaign spending.")
                
    # Best campaign
    best_cmp_data = next((c for c in campaigns_results if c["name"] == best_campaign), None)
    if best_cmp_data:
        insights.append(f"The best performing campaign by ROI is '{best_campaign}' ({round(best_cmp_data['roi']*100, 2)}% ROI).")
        
    # Worst campaign
    worst_cmp_data = next((c for c in campaigns_results if c["name"] == worst_campaign), None)
    if worst_cmp_data:
        insights.append(f"The lowest performing campaign by ROI is '{worst_campaign}' ({round(worst_cmp_data['roi']*100, 2)}% ROI).")

    # Education spending
    if 'Education' in df.columns and 'TotalSpent' in df.columns:
        edu_spend = df.groupby('Education')['TotalSpent'].mean()
        if not edu_spend.empty:
            top_edu_spend = edu_spend.idxmax()
            insights.append(f"Customers with '{top_edu_spend}' education profiles show the highest average total spend.")
            
    # Marital status purchases
    if 'Marital_Status' in df.columns and 'TotalPurchases' in df.columns:
        marital_purch = df.groupby('Marital_Status')['TotalPurchases'].mean()
        if not marital_purch.empty:
            top_marital_purch = marital_purch.idxmax()
            insights.append(f"Customers with marital status '{top_marital_purch}' represent the highest average purchase counts.")
            
    # Recency response relation
    if 'Recency' in df.columns and 'Response' in df.columns:
        recency_resp = df.groupby('Response')['Recency'].mean()
        if 1 in recency_resp.index and 0 in recency_resp.index:
            diff = recency_resp[0] - recency_resp[1]
            if diff > 0:
                insights.append(f"Responders have lower average recency ({round(recency_resp[1], 1)} days) than non-responders ({round(recency_resp[0], 1)} days), indicating active customers respond better.")

    # Build final structured response
    output_data = {
        "success": True,
        "dataset_summary": {
            "num_rows": num_rows,
            "num_columns": num_cols,
            "column_names": col_names,
            "dtypes": dtypes,
            "memory_usage_bytes": memory_usage
        },
        "missing_value_analysis": missing_analysis,
        "duplicate_analysis": {
            "total_duplicate_rows": num_duplicates,
            "duplicate_percentage": round(duplicate_percentage, 4)
        },
        "summary_statistics": summary_stats,
        "outlier_detection": outliers_info,
        "correlation_analysis": {
            "correlation_matrix": correlation_matrix,
            "strong_positive_correlations": strong_positive,
            "strong_negative_correlations": strong_negative
        },
        "customer_behaviour_insights": {
            "average_customer_income": round(avg_income, 2),
            "average_purchase_amount": round(avg_purchase_amount, 2),
            "average_number_of_purchases": round(avg_number_of_purchases, 2),
            "most_common_education": most_common_edu,
            "most_common_marital_status": most_common_marital,
            "top_spending_product_category": top_spend_category,
            "highest_revenue_marketing_channel": highest_rev_channel
        },
        "campaign_insights": {
            "total_campaigns": total_campaigns,
            "total_responses": total_responses,
            "response_rate": round(overall_response_rate, 4),
            "average_revenue": round(avg_revenue, 2),
            "average_spend": round(avg_spend, 2),
            "best_performing_campaign": best_campaign,
            "worst_performing_campaign": worst_campaign,
            "campaign_breakdown": campaigns_results
        },
        "business_insights": insights[:10]
    }
    
    return output_data

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(json.dumps({
            "success": False,
            "error": "Dataset file path argument missing. Usage: python eda.py <path_to_csv>"
        }))
        sys.exit(1)
        
    csv_file_path = sys.argv[1]
    analysis_result = run_eda(csv_file_path)
    
    # Output only raw JSON string to stdout
    print(json.dumps(analysis_result))
