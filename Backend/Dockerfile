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
