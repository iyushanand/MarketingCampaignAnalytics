# Deployment & Production Release Guide

This guide describes the end-to-end deployment strategies for the **Marketing Campaign Analytics Platform** across local development, containerized Docker, and cloud-native (Azure) environments.

---

## 1. Local Production Mock Deployment

For simulating a production release locally, follow this configuration:

### Database (SQL Server LocalDB / Express)
1. Ensure **SQL Server LocalDB** or **SQL Server Express** is running:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```
2. The C# Web API is configured to automatically run EF Core migrations and seed the database on startup. Ensure the connection string in `Backend/appsettings.json` points to the running instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MarketingCampaignAnalytics;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

### Backend (ASP.NET Core Web API)
1. Publish the Web API executable:
   ```powershell
   dotnet publish Backend/Backend.csproj -c Release -o ./publish/backend
   ```
2. Ensure Python is installed and accessible from the hosting environment. Verify Python path settings in `./publish/backend/appsettings.json`:
   ```json
   "PythonSettings": {
     "PythonPath": "python"
   }
   ```
3. Run the backend service:
   ```powershell
   cd ./publish/backend
   ./Backend.exe --urls "http://localhost:5224"
   ```

### Frontend (Angular Client)
1. Build the production client bundles:
   ```powershell
   cd Frontend
   npm run build -- --configuration production
   ```
2. The output bundles are generated under `Frontend/dist/frontend/browser/`.
3. Host these files using a web server like **IIS**, **Apache**, **Nginx**, or a simple static server:
   ```powershell
   npm install -g serve
   serve -s dist/frontend/browser -l 4200
   ```

---

## 2. Containerized Deployment (Docker & Docker Compose)

To run the entire multi-service stack in isolated containers, utilize the following setup.

### 1. Backend Dockerfile (`Backend/Dockerfile`)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source files and build release
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Install Python and analytical dependencies
RUN apt-get update && apt-get install -y python3 python3-pip python3-venv
RUN python3 -m venv /opt/venv
ENV PATH="/opt/venv/bin:$PATH"
RUN pip install pandas numpy scikit-learn scipy statsmodels openpyxl reportlab joblib

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5224
ENV PythonSettings__PythonPath=python3

EXPOSE 5224
ENTRYPOINT ["dotnet", "Backend.dll"]
```

### 2. Frontend Dockerfile (`Frontend/Dockerfile`)
```dockerfile
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 3. Orchestration Configuration (`docker-compose.yml`)
```yaml
version: '3.8'

services:
  database:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: mca_database
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrongPassword123!
    ports:
      - "1433:1433"
    volumes:
      - mca_db_data:/var/opt/mssql

  backend:
    build:
      context: ./Backend
      dockerfile: Dockerfile
    container_name: mca_backend
    environment:
      - ConnectionStrings__DefaultConnection=Server=database;Database=MarketingCampaignAnalytics;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;
    ports:
      - "5224:5224"
    depends_on:
      - database

  frontend:
    build:
      context: ./Frontend
      dockerfile: Dockerfile
    container_name: mca_frontend
    ports:
      - "4200:80"
    depends_on:
      - backend

volumes:
  mca_db_data:
```

---

## 3. Cloud-Native Deployment (Microsoft Azure)

This platform is structured for native hosting on the Microsoft Azure Cloud.

### Architecture Topology
```text
  [ Azure Static Web App ]  ◄── HTTPS ──►  [ Azure App Service (C# API) ]
          (Frontend)                                      │
                                                   (Process Execution)
                                                          ▼
  [ Azure SQL Database ]   ◄───────────────────  [ Python Runtime ]
     (Relational DB)
```

### Azure Deployment Steps:
1. **Azure SQL Database**:
   - Provision a logical server and database (Serverless tier is recommended to optimize costs).
   - Configure Firewall rules to "Allow Azure services and resources to access this server."
   - Retrieve the ADO.NET connection string.
2. **Azure App Service (Backend)**:
   - Create a Linux App Service Plan running **.NET 8**.
   - Under Settings -> Configuration, add Connection String `DefaultConnection` with your Azure SQL connection.
   - Install Python on App Service or host python logic inside **Azure Functions (Python)** and modify `PythonRunner` to forward requests via REST API (Enterprise Best Practice for high scale).
   - Alternatively, deploy the Backend using a custom **Docker Container** on Azure App Service.
3. **Azure Static Web App (Frontend)**:
   - Connect your GitHub Repository to Azure Static Web Apps.
   - Configure the workflow file to build Angular:
     - `app_location`: `"/Frontend"`
     - `output_location`: `"dist/frontend/browser"`
   - Configure Route Rewriting in `staticwebapp.config.json` to handle Angular routing redirects.
