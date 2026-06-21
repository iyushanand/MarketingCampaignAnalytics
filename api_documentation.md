# API Endpoints Documentation

This document describes all RESTful API endpoints exposed by the Marketing Campaign Analytics Platform.

---

## Base API Configurations
- **Development Host**: `http://localhost:5224`
- **Default Format**: `application/json`
- **Error Format**: Returns `ApiResponse<T>` wrapper where `Success = false` and `Message` details the fault.

---

## 1. Customer Analytics API (`/api/customer`)

### GET `/api/customer`
Retrieves the list of customers extended with RFM properties and response rates.
- **Response Code**: `200 OK`
- **Output JSON**:
  ```json
  {
    "success": true,
    "message": "Success",
    "data": [
      {
        "customerId": 1,
        "firstName": "John",
        "lastName": "Smith",
        "gender": "Male",
        "age": 42,
        "income": 54000.00,
        "education": "Graduation",
        "country": "United States",
        "recency": 15,
        "frequency": 12,
        "monetary": 450.00,
        "rfmSegment": "Medium Value",
        "responseRate": 0.1667
      }
    ]
  }
  ```

### GET `/api/customer/summary`
Gets overall customer KPIs, behavior averages, and lists top spenders/active profiles.
- **Response Code**: `200 OK`

### GET `/api/customer/personas`
Retrieves mapped marketing personas (High Value, Frequent Buyers, Occasional Buyers, At Risk) with spend profiles.
- **Response Code**: `200 OK`

### GET `/api/customer/analytics`
Gets age, gender, education, country, and income distributions, and spending/response rate cross-sectional bins.
- **Response Code**: `200 OK`

---

## 2. Response Prediction API (`/api/prediction`)

### POST `/api/prediction/train`
Triggers database export and execution of Python training script. Generates `model.pkl` and `model_metrics.json`.
- **Response Code**: `200 OK`
- **Output JSON**:
  ```json
  {
    "success": true,
    "message": "Model trained successfully."
  }
  ```

### POST `/api/prediction`
Executes classification inference on a single request profile. Returns verdict, probability, confidence tier, and dynamic explanations.
- **Input JSON**:
  ```json
  {
    "age": 42,
    "income": 65000.00,
    "education": "PhD",
    "totalPurchases": 15,
    "averageSpend": 85.50,
    "campaignChannel": "Email"
  }
  ```
- **Response Code**: `200 OK`
- **Output JSON**:
  ```json
  {
    "success": true,
    "message": "Prediction succeeded.",
    "data": {
      "prediction": "Likely Response",
      "probability": 0.8652,
      "confidenceLevel": "High",
      "businessReasons": [
        "High purchase frequency increases campaign response likelihood.",
        "Customer average spend is above the market average.",
        "Email campaign channel historically achieves high engagement rates."
      ]
    }
  }
  ```

### GET `/api/prediction/metrics`
Reads the metrics JSON file and returns performance indicators.
- **Response Code**: `200 OK`

---

## 3. Automated Report API (`/api/reports`)

### GET `/api/reports/excel`
Generates and downloads the professional Excel workbook.
- **Response Code**: `200 OK`
- **Content-Type**: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`

### GET `/api/reports/pdf`
Generates and downloads the executive PDF summary.
- **Response Code**: `200 OK`
- **Content-Type**: `application/pdf`

### GET `/api/reports/list`
Lists previously generated report files.
- **Response Code**: `200 OK`

### GET `/api/reports/download?fileName={name}`
Streams an archived report file.
- **Response Code**: `200 OK` / `404 NotFound`

---

## 4. Tableau Integration API (`/api/tableau`)

### GET `/api/tableau/export/all`
Triggers exporting of the 4 Tableau CSV datasets.
- **Response Code**: `200 OK`

### GET `/api/tableau/export/{campaign|customer|summary|monthly}`
Exports and downloads a specific CSV file.
- **Response Code**: `200 OK`
