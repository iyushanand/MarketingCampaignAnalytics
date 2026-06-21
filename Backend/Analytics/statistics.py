import sys
import os
import json
import warnings
import pandas as pd
import numpy as np
import scipy.stats as stats

# Suppress all warnings
warnings.filterwarnings("ignore")

def compute_confidence_interval(data, confidence=0.95):
    """
    Computes the confidence interval for a 1D numeric array.
    """
    series = pd.Series(data).dropna()
    n = len(series)
    if n <= 1:
        return 0.0, 0.0, 0.0
    mean = float(series.mean())
    sem = stats.sem(series)
    margin = sem * stats.t.ppf((1 + confidence) / 2.0, n - 1)
    return round(mean - margin, 4), round(mean + margin, 4), round(mean, 4)

def run_statistics(csv_path):
    """
    Performs statistical tests on the dataset and returns results in structured JSON.
    """
    if not os.path.exists(csv_path):
        return {
            "success": False,
            "error": f"File not found: {csv_path}"
        }
    
    try:
        # Detect delimiter
        with open(csv_path, 'r', encoding='utf-8', errors='ignore') as f:
            first_line = f.readline()
        
        separator = '\t' if '\t' in first_line else ','
        df = pd.read_csv(csv_path, sep=separator)
    except Exception as e:
        return {
            "success": False,
            "error": f"Failed to read CSV: {str(e)}"
        }
    
    # Validate required columns
    required_cols = [
        "Year_Birth", "Income", "Education", "Marital_Status", "Response",
        "MntWines", "MntFruits", "MntMeatProducts", "MntFishProducts", "MntSweetProducts", "MntGoldProds",
        "NumDealsPurchases", "NumWebPurchases", "NumCatalogPurchases", "NumStorePurchases"
    ]
    missing_req = [col for col in required_cols if col not in df.columns]
    if missing_req:
        return {
            "success": False,
            "error": f"Missing required columns: {', '.join(missing_req)}"
        }
        
    num_rows = len(df)
    if num_rows < 10:
        return {
            "success": False,
            "error": "Dataset is too small to perform reliable statistical testing."
        }
    
    # Derivations
    df['Age'] = 2026 - df['Year_Birth']
    df['TotalSpent'] = df[['MntWines', 'MntFruits', 'MntMeatProducts', 'MntFishProducts', 'MntSweetProducts', 'MntGoldProds']].sum(axis=1)
    df['TotalPurchases'] = df[['NumWebPurchases', 'NumCatalogPurchases', 'NumStorePurchases']].sum(axis=1)
    
    tests_performed = 0
    sig_findings = 0
    
    # 1. Independent T-Test (Response = 1 vs 0 on TotalSpent)
    t_test_results = {}
    try:
        group_yes = df[df['Response'] == 1]['TotalSpent'].dropna()
        group_no = df[df['Response'] == 0]['TotalSpent'].dropna()
        
        mean_yes = float(group_yes.mean()) if len(group_yes) > 0 else 0.0
        mean_no = float(group_no.mean()) if len(group_no) > 0 else 0.0
        
        if len(group_yes) > 1 and len(group_no) > 1:
            t_stat, p_val = stats.ttest_ind(group_yes, group_no, equal_var=False)
            t_stat = float(t_stat)
            p_val = float(p_val)
            is_significant = p_val < 0.05
            tests_performed += 1
            if is_significant:
                sig_findings += 1
                
            interpretation = (
                f"The difference in average purchase amount between responders (${round(mean_yes, 2)}) "
                f"and non-responders (${round(mean_no, 2)}) is statistically significant (p = {p_val:.4f} < 0.05). "
                "Active responders show a significantly higher purchase behavior."
            ) if is_significant else (
                f"The difference in average purchase amount between responders (${round(mean_yes, 2)}) "
                f"and non-responders (${round(mean_no, 2)}) is not statistically significant (p = {p_val:.4f} >= 0.05)."
            )
        else:
            t_stat, p_val, is_significant = 0.0, 1.0, False
            interpretation = "Insufficient data in responder groups to perform T-test."
            
        t_test_results = {
            "group_yes_mean": round(mean_yes, 2),
            "group_no_mean": round(mean_no, 2),
            "t_statistic": round(t_stat, 4),
            "p_value": round(p_val, 6),
            "is_significant": is_significant,
            "interpretation": interpretation
        }
    except Exception as e:
        t_test_results = {"error": f"T-Test failed: {str(e)}"}

    # 2. Chi-Square Test (Education vs Response)
    chi_square_results = {}
    try:
        contingency = pd.crosstab(df['Education'], df['Response'])
        if contingency.shape[0] > 1 and contingency.shape[1] > 1:
            chi2, p_val, dof, expected = stats.chi2_contingency(contingency)
            chi2 = float(chi2)
            p_val = float(p_val)
            dof = int(dof)
            expected_list = expected.tolist()
            
            is_significant = p_val < 0.05
            tests_performed += 1
            if is_significant:
                sig_findings += 1
                
            interpretation = (
                f"There is a statistically significant association between Education Level and Campaign Response "
                f"(chi-square = {chi2:.2f}, p = {p_val:.4f} < 0.05). Customer segment responsiveness varies by education background."
            ) if is_significant else (
                f"There is no statistically significant association between Education Level and Campaign Response "
                f"(chi-square = {chi2:.2f}, p = {p_val:.4f} >= 0.05)."
            )
            
            chi_square_results = {
                "chi_square_statistic": round(chi2, 4),
                "degrees_of_freedom": dof,
                "p_value": round(p_val, 6),
                "expected_frequencies": expected_list,
                "is_significant": is_significant,
                "interpretation": interpretation
            }
        else:
            chi_square_results = {"error": "Contingency table dimensions are insufficient for Chi-square."}
    except Exception as e:
        chi_square_results = {"error": f"Chi-Square failed: {str(e)}"}

    # 3. Correlation Analysis (Pearson)
    corr_results = {}
    highest_corr_var1, highest_corr_var2, highest_corr_coef = "N/A", "N/A", 0.0
    lowest_corr_var1, lowest_corr_var2, lowest_corr_coef = "N/A", "N/A", 1.0
    try:
        cols_to_exclude = ["ID", "Z_CostContact", "Z_Revenue"]
        numeric_cols = [c for c in df.select_dtypes(include=[np.number]).columns if c not in cols_to_exclude]
        
        corr_matrix_df = df[numeric_cols].corr(method='pearson').fillna(0.0)
        
        # Format matrix for JSON output
        corr_matrix = {}
        for col1 in corr_matrix_df.index:
            corr_matrix[col1] = {}
            for col2 in corr_matrix_df.columns:
                corr_matrix[col1][col2] = float(corr_matrix_df.loc[col1, col2])
                
        strong_pos = []
        strong_neg = []
        moderate = []
        weak = []
        
        cols = list(corr_matrix_df.columns)
        for i in range(len(cols)):
            for j in range(i + 1, len(cols)):
                col1 = cols[i]
                col2 = cols[j]
                coef = float(corr_matrix_df.loc[col1, col2])
                
                # Update highest/lowest correlated variables
                if coef > highest_corr_coef:
                    highest_corr_coef = coef
                    highest_corr_var1, highest_corr_var2 = col1, col2
                if abs(coef) < abs(lowest_corr_coef):
                    lowest_corr_coef = coef
                    lowest_corr_var1, lowest_corr_var2 = col1, col2
                
                rel = {"feature1": col1, "feature2": col2, "coefficient": round(coef, 4)}
                if coef > 0.70:
                    strong_pos.append(rel)
                elif coef < -0.70:
                    strong_neg.append(rel)
                elif abs(coef) >= 0.30:
                    moderate.append(rel)
                else:
                    weak.append(rel)
                    
        # Sort relationships
        strong_pos = sorted(strong_pos, key=lambda x: x["coefficient"], reverse=True)
        strong_neg = sorted(strong_neg, key=lambda x: x["coefficient"])
        
        # Build business explanation
        interpretation = "Correlation analysis indicates spend categories and income profiles move closely together. "
        if strong_pos:
            interpretation += f"Specifically, {strong_pos[0]['feature1']} and {strong_pos[0]['feature2']} show the strongest positive correlation (r = {strong_pos[0]['coefficient']})."
        else:
            interpretation += "No exceptionally strong linear relationships (r > 0.70) were detected."
            
        corr_results = {
            "correlation_matrix": corr_matrix,
            "strong_positive_relationships": strong_pos[:5],
            "strong_negative_relationships": strong_neg[:5],
            "moderate_relationships": moderate[:10],
            "weak_relationships": weak[:10],
            "interpretation": interpretation
        }
    except Exception as e:
        corr_results = {"error": f"Correlation analysis failed: {str(e)}"}

    # 4. Linear Regression (Predict TotalSpent using Income, Age, TotalPurchases)
    regression_results = {}
    strongest_predictor = "N/A"
    weakest_predictor = "N/A"
    try:
        # Pre-clean dataset for OLS
        reg_df = df[['TotalSpent', 'Income', 'Age', 'TotalPurchases']].dropna()
        n = len(reg_df)
        p = 3  # number of predictors
        
        if n > p + 1:
            y = reg_df['TotalSpent'].values
            X_vals = reg_df[['Income', 'Age', 'TotalPurchases']].values
            X = np.column_stack((np.ones(n), X_vals))
            
            # Solve OLS via math: beta = (X^T * X)^-1 * X^T * y
            XtX = X.T @ X
            if np.linalg.det(XtX) != 0:
                beta = np.linalg.inv(XtX) @ X.T @ y
                y_pred = X @ beta
                residuals = y - y_pred
                
                sst = np.sum((y - np.mean(y))**2)
                sse = np.sum(residuals**2)
                
                r_squared = 1.0 - (sse / sst) if sst > 0 else 0.0
                adj_r_squared = 1.0 - (1.0 - r_squared) * (n - 1) / (n - p - 1) if sst > 0 else 0.0
                
                df_resid = n - p - 1
                mse = sse / df_resid if df_resid > 0 else 0.0
                
                # Covariance matrix of coefficients
                var_beta = mse * np.linalg.inv(XtX)
                se_beta = np.sqrt(np.maximum(np.diagonal(var_beta), 0.0))
                
                t_stats = np.zeros(len(beta))
                p_values_list = np.ones(len(beta))
                for k in range(len(beta)):
                    if se_beta[k] > 0:
                        t_stats[k] = beta[k] / se_beta[k]
                        p_values_list[k] = 2.0 * (1.0 - stats.t.cdf(abs(t_stats[k]), df_resid))
                
                coefs = {
                    "const": float(beta[0]),
                    "Income": float(beta[1]),
                    "Age": float(beta[2]),
                    "TotalPurchases": float(beta[3])
                }
                p_values = {
                    "const": float(p_values_list[0]),
                    "Income": float(p_values_list[1]),
                    "Age": float(p_values_list[2]),
                    "TotalPurchases": float(p_values_list[3])
                }
                
                # Calculate standardized coefficients to determine relative strength
                std_y = y.std()
                std_coefs = {}
                for idx, col in enumerate(['Income', 'Age', 'TotalPurchases']):
                    std_x = X_vals[:, idx].std()
                    std_coef = beta[idx + 1] * (std_x / std_y) if std_y > 0 else 0.0
                    std_coefs[col] = abs(std_coef)
                    
                sorted_predictors = sorted(std_coefs.items(), key=lambda item: item[1], reverse=True)
                strongest_predictor = sorted_predictors[0][0]
                weakest_predictor = sorted_predictors[-1][0]
                
                tests_performed += 1
                # Overall F-test p-value (approximate check using F distribution)
                # F = (SSR/p) / (SSE/df_resid)
                ssr = sst - sse
                f_stat = (ssr / p) / (sse / df_resid) if sse > 0 else 0.0
                f_p_val = 1.0 - stats.f.cdf(f_stat, p, df_resid)
                if f_p_val < 0.05:
                    sig_findings += 1
                    
                equation = f"TotalSpent = {coefs['const']:.2f} + ({coefs['Income']:.4f} * Income) + ({coefs['Age']:.2f} * Age) + ({coefs['TotalPurchases']:.2f} * TotalPurchases)"
                interpretation = (
                    f"The regression model explains {r_squared*100:.1f}% of the variance in customer spending (R-squared = {r_squared:.3f}, p < 0.05). "
                    f"'{strongest_predictor}' is the strongest predictor of customer spend, showing a significant influence on purchase amounts."
                )
                
                regression_results = {
                    "regression_equation": equation,
                    "coefficients": {k: round(v, 4) for k, v in coefs.items()},
                    "intercept": round(coefs['const'], 4),
                    "r_squared": round(r_squared, 4),
                    "adjusted_r_squared": round(adj_r_squared, 4),
                    "p_values": {k: round(v, 6) for k, v in p_values.items()},
                    "interpretation": interpretation
                }
            else:
                regression_results = {"error": "Regression matrix is singular and cannot be inverted."}
        else:
            regression_results = {"error": "Insufficient dataset records left after removing missing income parameters."}
    except Exception as e:
        regression_results = {"error": f"Linear regression model fit failed: {str(e)}"}

    # 5. Confidence Intervals (95%)
    confidence_intervals = {}
    try:
        spent_lower, spent_upper, spent_mean = compute_confidence_interval(df['TotalSpent'])
        income_lower, income_upper, income_mean = compute_confidence_interval(df['Income'])
        
        # Campaign revenues (1/3 spend allocation model consistent with Phase 5 eda.py)
        campaigns_info = [
            {"col": "AcceptedCmp1"},
            {"col": "AcceptedCmp2"},
            {"col": "AcceptedCmp3"},
            {"col": "AcceptedCmp4"},
            {"col": "AcceptedCmp5"},
            {"col": "Response"}
        ]
        
        campaign_revenues = []
        for c in campaigns_info:
            col = c["col"]
            if col in df.columns:
                revenue = float(df[df[col] == 1]['TotalSpent'].sum() / 3.0)
                campaign_revenues.append(revenue)
                
        rev_lower, rev_upper, rev_mean = compute_confidence_interval(campaign_revenues)
        
        confidence_intervals = {
            "average_purchase_amount": {
                "lower_bound": spent_lower,
                "upper_bound": spent_upper,
                "mean": spent_mean
            },
            "average_income": {
                "lower_bound": income_lower,
                "upper_bound": income_upper,
                "mean": income_mean
            },
            "average_campaign_revenue": {
                "lower_bound": rev_lower,
                "upper_bound": rev_upper,
                "mean": rev_mean
            }
        }
    except Exception as e:
        confidence_intervals = {"error": f"Confidence interval calculations failed: {str(e)}"}

    # 6. Statistical Summary
    summary = {
        "number_of_statistical_tests_performed": tests_performed,
        "number_of_statistically_significant_findings": sig_findings,
        "strongest_predictor": strongest_predictor,
        "weakest_predictor": weakest_predictor,
        "highest_correlated_variables": {
            "variable1": highest_corr_var1,
            "variable2": highest_corr_var2,
            "coefficient": round(highest_corr_coef, 4)
        },
        "lowest_correlated_variables": {
            "variable1": lowest_corr_var1,
            "variable2": lowest_corr_var2,
            "coefficient": round(lowest_corr_coef, 4)
        }
    }

    # 7. Business Recommendations
    recommendations = []
    
    # Recommendation 1: T-test responder segment
    if t_test_results.get("is_significant"):
        recommendations.append(
            f"Increase budget allocation toward the highly responsive customer segment. Responders spend significantly "
            f"more on average (${t_test_results['group_yes_mean']}) than non-responders (${t_test_results['group_no_mean']})."
        )
    
    # Recommendation 2: Chi-square education targeting
    if chi_square_results.get("is_significant"):
        recommendations.append(
            "Tailor campaign creatives and channel selections based on the customer's educational background, "
            "as Chi-Square tests indicate a statistically significant association between Education level and response behavior."
        )
        
    # Recommendation 3: Linear regression predictor
    if strongest_predictor != "N/A":
        recommendations.append(
            f"Focus customer acquisition efforts and persona building on customer profiles with higher values of '{strongest_predictor}', "
            f"as this was identified as the strongest statistical predictor of total customer spend."
        )
        
    # Recommendation 4: Income confidence interval targeting
    if "average_income" in confidence_intervals and confidence_intervals["average_income"]["lower_bound"] > 0:
        lower_inc = confidence_intervals["average_income"]["lower_bound"]
        recommendations.append(
            f"Position luxury or high-margin product offerings (such as premium wines and meats) toward segments "
            f"exhibiting incomes within the statistical confidence range of ${lower_inc:,.2f} to ${confidence_intervals['average_income']['upper_bound']:,.2f}."
        )
        
    # Recommendation 5: Strongest correlation category
    if highest_corr_var1 != "N/A" and highest_corr_coef > 0.5:
        recommendations.append(
            f"Design bundle promotions or cross-selling strategies linking '{highest_corr_var1}' and '{highest_corr_var2}', "
            f"due to a strong positive correlation coefficient of {highest_corr_coef:.2f}."
        )
        
    # Recommendation 6: Weakest predictor
    if weakest_predictor != "N/A":
        recommendations.append(
            f"De-emphasize '{weakest_predictor}' as a core targeting criterion in premium spend modeling, "
            f"since regression indicates it has the weakest influence on overall customer purchase amounts."
        )
        
    business_recommendations = recommendations[:8]

    # Combine everything to final JSON structure
    output_data = {
        "t_test": t_test_results,
        "chi_square": chi_square_results,
        "correlation": corr_results,
        "linear_regression": regression_results,
        "confidence_intervals": confidence_intervals,
        "summary": summary,
        "business_recommendations": business_recommendations
    }
    
    return output_data

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(json.dumps({
            "success": False,
            "error": "Dataset file path argument missing. Usage: python statistics.py <path_to_csv>"
        }))
        sys.exit(1)
        
    csv_file_path = sys.argv[1]
    stats_result = run_statistics(csv_file_path)
    
    print(json.dumps(stats_result))
