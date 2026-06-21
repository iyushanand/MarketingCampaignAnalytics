import sys
import os
import json
import warnings
import pandas as pd
import numpy as np
import joblib
from sklearn.model_selection import train_test_split
from sklearn.compose import ColumnTransformer
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from sklearn.impute import SimpleImputer
from sklearn.pipeline import Pipeline
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score, precision_score, recall_score, f1_score, roc_auc_score, confusion_matrix, classification_report

# Suppress runtime warnings
warnings.filterwarnings("ignore")

def train_model(csv_path, model_path, metrics_path):
    """
    Trains a Logistic Regression pipeline on the campaign response dataset
    and saves the trained model (.pkl) and evaluation metrics (.json).
    """
    if not os.path.exists(csv_path):
        return {"success": False, "error": f"Training CSV file not found: {csv_path}"}
    
    try:
        # Load dataset
        df = pd.read_csv(csv_path)
        
        # Define features and target
        feature_cols = ["Age", "Income", "Education", "TotalPurchases", "AverageSpend", "CampaignChannel"]
        target_col = "Response"
        
        # Verify required columns exist
        missing_cols = [c for c in feature_cols + [target_col] if c not in df.columns]
        if missing_cols:
            return {"success": False, "error": f"Missing required columns: {', '.join(missing_cols)}"}
            
        X = df[feature_cols]
        y = df[target_col]
        
        # Train-Test Split (80% / 20%)
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.20, random_state=42, stratify=y)
        
        # Preprocessing Pipeline
        numeric_features = ["Age", "Income", "TotalPurchases", "AverageSpend"]
        numeric_transformer = Pipeline(steps=[
            ("imputer", SimpleImputer(strategy="median")),
            ("scaler", StandardScaler())
        ])
        
        categorical_features = ["Education", "CampaignChannel"]
        categorical_transformer = Pipeline(steps=[
            ("imputer", SimpleImputer(strategy="most_frequent")),
            ("onehot", OneHotEncoder(handle_unknown="ignore"))
        ])
        
        preprocessor = ColumnTransformer(
            transformers=[
                ("num", numeric_transformer, numeric_features),
                ("cat", categorical_transformer, categorical_features)
            ]
        )
        
        # Logistic Regression Pipeline
        pipeline = Pipeline(steps=[
            ("preprocessor", preprocessor),
            ("classifier", LogisticRegression(max_iter=1000, random_state=42))
        ])
        
        # Train model
        pipeline.fit(X_train, y_train)
        
        # Evaluate model
        y_pred = pipeline.predict(X_test)
        y_prob = pipeline.predict_proba(X_test)[:, 1]
        
        accuracy = accuracy_score(y_test, y_pred)
        precision = precision_score(y_test, y_pred, zero_division=0)
        recall = recall_score(y_test, y_pred, zero_division=0)
        f1 = f1_score(y_test, y_pred, zero_division=0)
        roc_auc = roc_auc_score(y_test, y_prob)
        conf_mat = confusion_matrix(y_test, y_pred).tolist()
        class_rep = classification_report(y_test, y_pred, zero_division=0)
        
        # Save model
        joblib.dump(pipeline, model_path)
        
        # Save metrics
        metrics = {
            "accuracy": round(float(accuracy), 4),
            "precision": round(float(precision), 4),
            "recall": round(float(recall), 4),
            "f1Score": round(float(f1), 4),
            "rocAuc": round(float(roc_auc), 4),
            "confusionMatrix": conf_mat,
            "classificationReport": class_rep
        }
        
        with open(metrics_path, "w", encoding="utf-8") as f:
            json.dump(metrics, f, indent=4)
            
        return {"success": True, "metrics": metrics}
        
    except Exception as e:
        return {"success": False, "error": f"Failed to train model: {str(e)}"}

def predict_single(model_path, age, income, education, total_purchases, average_spend, campaign_channel):
    """
    Performs inference using the saved Logistic Regression pipeline
    and returns a prediction, probability, confidence, and rule-based explanations.
    """
    if not os.path.exists(model_path):
        return {"success": False, "error": "Model file not found. Please train the model first."}
        
    try:
        # Load pipeline
        pipeline = joblib.load(model_path)
        
        # Prepare single row DataFrame
        input_data = pd.DataFrame([{
            "Age": int(age),
            "Income": float(income),
            "Education": str(education),
            "TotalPurchases": int(total_purchases),
            "AverageSpend": float(average_spend),
            "CampaignChannel": str(campaign_channel)
        }])
        
        # Predict
        pred_val = int(pipeline.predict(input_data)[0])
        prob_val = float(pipeline.predict_proba(input_data)[0][1])
        
        prediction = "Likely Response" if pred_val == 1 else "Not Likely Response"
        
        # Confidence rating
        confidence = "High"
        if 0.40 <= prob_val <= 0.60:
            confidence = "Low"
        elif 0.30 <= prob_val <= 0.70:
            confidence = "Medium"
            
        # Business Explanations / Reasons
        reasons = []
        
        # Rule 1: Purchases / Frequency
        if int(total_purchases) >= 12:
            reasons.append("High purchase frequency increases campaign response likelihood.")
        elif int(total_purchases) < 5:
            reasons.append("Low purchase frequency reduces overall response probability.")
            
        # Rule 2: Spend
        if float(average_spend) >= 150:
            reasons.append("Average spending value is above the average customer profile.")
        elif float(average_spend) < 30:
            reasons.append("Low average spending limits buyer responsiveness margins.")

        # Rule 3: Segment / Income
        if float(income) >= 70000:
            reasons.append("Customer belongs to a premium income tier historically matching high response.")
        elif float(income) < 30000:
            reasons.append("Lower income demographic tier reduces overall campaign conversions.")

        # Rule 4: Channel historical weight
        if str(campaign_channel).lower() in ["email", "sms"]:
            reasons.append("Campaign channel (Email/SMS) historically shows strong conversion rates.")
        else:
            reasons.append("Secondary marketing channel displays lower baseline response frequency.")

        # Cap explanations to 3-5
        if len(reasons) < 3:
            reasons.append("Demographic traits match consistent average portfolio behavior.")
            
        return {
            "success": True,
            "prediction": prediction,
            "probability": round(prob_val, 4),
            "confidenceLevel": confidence,
            "businessReasons": reasons[:4]
        }
        
    except Exception as e:
        return {"success": False, "error": f"Prediction failed: {str(e)}"}

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(json.dumps({"success": False, "error": "Arguments missing. Usage: train or predict."}))
        sys.exit(1)
        
    mode = sys.argv[1].lower()
    
    if mode == "train":
        if len(sys.argv) < 5:
            print(json.dumps({"success": False, "error": "Usage: python machine_learning.py train <csv_path> <model_path> <metrics_path>"}))
            sys.exit(1)
        csv_file = sys.argv[2]
        model_file = sys.argv[3]
        metrics_file = sys.argv[4]
        res = train_model(csv_file, model_file, metrics_file)
        print(json.dumps(res))
        
    elif mode == "predict":
        if len(sys.argv) < 9:
            print(json.dumps({"success": False, "error": "Usage: python machine_learning.py predict <model_path> <age> <income> <education> <total_purchases> <avg_spend> <campaign_channel>"}))
            sys.exit(1)
        model_file = sys.argv[2]
        age = sys.argv[3]
        income = sys.argv[4]
        education = sys.argv[5]
        total_purchases = sys.argv[6]
        avg_spend = sys.argv[7]
        channel = sys.argv[8]
        res = predict_single(model_file, age, income, education, total_purchases, avg_spend, channel)
        print(json.dumps(res))
    else:
        print(json.dumps({"success": False, "error": f"Unknown mode: {mode}"}))
        sys.exit(1)
