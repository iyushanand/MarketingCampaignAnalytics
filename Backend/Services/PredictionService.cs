using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.Models;
using Backend.Repository;

namespace Backend.Services
{
    /// <summary>
    /// Service implementation for Machine Learning Response Prediction.
    /// </summary>
    public class PredictionService : IPredictionService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICampaignResponseRepository _responseRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly PythonRunner _pythonRunner;

        public PredictionService(
            ICustomerRepository customerRepository,
            ICampaignResponseRepository responseRepository,
            ICampaignRepository campaignRepository,
            PythonRunner pythonRunner)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _pythonRunner = pythonRunner ?? throw new ArgumentNullException(nameof(pythonRunner));
        }

        /// <summary>
        /// Generates a training CSV from SQL Server and runs the Python training pipeline.
        /// </summary>
        public async Task<bool> TrainModelAsync()
        {
            try
            {
                var customers = (await _customerRepository.GetAllAsync()).ToList();
                var responses = (await _responseRepository.GetAllAsync()).ToList();
                var campaigns = (await _campaignRepository.GetAllAsync()).ToList();

                if (!customers.Any() || !responses.Any())
                {
                    return false; // No data to train on
                }

                // Compute customer total purchases and average spend overall
                var customerStats = responses.GroupBy(x => x.CustomerId)
                    .ToDictionary(g => g.Key, g => {
                        int totalPurchases = g.Sum(x => x.NumberOfPurchases);
                        decimal totalSpend = g.Sum(x => x.PurchaseAmount);
                        decimal avgSpend = totalPurchases > 0 ? totalSpend / totalPurchases : 0m;
                        return new { TotalPurchases = totalPurchases, AverageSpend = avgSpend };
                    });

                // Generate training rows joining CampaignResponse with Customer and Campaign
                var trainingRows = new List<string> { "Age,Income,Education,TotalPurchases,AverageSpend,CampaignChannel,Response" };
                
                foreach (var r in responses)
                {
                    var c = customers.FirstOrDefault(x => x.CustomerId == r.CustomerId);
                    var cp = campaigns.FirstOrDefault(x => x.CampaignId == r.CampaignId);
                    if (c == null || cp == null) continue;

                    var stats = customerStats.GetValueOrDefault(r.CustomerId);
                    int totalPurch = stats?.TotalPurchases ?? 0;
                    decimal avgSpend = stats?.AverageSpend ?? 0m;

                    string eduEscaped = c.Education.Replace("\"", "\"\"");
                    string channelEscaped = cp.MarketingChannel.Replace("\"", "\"\"");
                    int responseLabel = r.Response == "Yes" ? 1 : 0;

                    trainingRows.Add($"{c.Age},{Math.Round(c.Income, 2)},\"{eduEscaped}\",{totalPurch},{Math.Round(avgSpend, 2)},\"{channelEscaped}\",{responseLabel}");
                }

                // Write training data to data/training_data.csv
                string csvPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "training_data.csv");
                Directory.CreateDirectory(Path.GetDirectoryName(csvPath)!);
                await File.WriteAllLinesAsync(csvPath, trainingRows);

                // Resolve Analytics folder directory for ML assets
                string analyticsDir = Path.Combine(Directory.GetCurrentDirectory(), "Analytics");
                if (!Directory.Exists(analyticsDir))
                {
                    analyticsDir = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "Analytics");
                }
                
                string modelPath = Path.Combine(analyticsDir, "model.pkl");
                string metricsPath = Path.Combine(analyticsDir, "model_metrics.json");

                // Execute training script in Python runner
                string resultJson = await _pythonRunner.RunScriptAsync(
                    "machine_learning.py", 
                    "train", 
                    csvPath, 
                    modelPath, 
                    metricsPath
                );

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<PythonResultHelper>(resultJson, options);
                
                return result?.Success ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Predicts response likelihood for a customer profile.
        /// </summary>
        public async Task<PredictionResponseDto> PredictAsync(PredictionRequestDto request)
        {
            try
            {
                string analyticsDir = Path.Combine(Directory.GetCurrentDirectory(), "Analytics");
                if (!Directory.Exists(analyticsDir))
                {
                    analyticsDir = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "Analytics");
                }

                string modelPath = Path.Combine(analyticsDir, "model.pkl");
                if (!File.Exists(modelPath))
                {
                    // Train the model on demand if not present
                    bool trained = await TrainModelAsync();
                    if (!trained || !File.Exists(modelPath))
                    {
                        return new PredictionResponseDto
                        {
                            Prediction = "Not Likely Response",
                            Probability = 0.0,
                            ConfidenceLevel = "Low",
                            BusinessReasons = new List<string> { "Model is currently untrained. Load the sample dataset first." }
                        };
                    }
                }

                string[] args = new string[]
                {
                    "predict",
                    modelPath,
                    request.Age.ToString(),
                    request.Income.ToString("F2"),
                    request.Education,
                    request.TotalPurchases.ToString(),
                    request.AverageSpend.ToString("F2"),
                    request.CampaignChannel
                };

                string resultJson = await _pythonRunner.RunScriptAsync("machine_learning.py", args);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<PredictionResultHelper>(resultJson, options);

                if (result == null || !result.Success)
                {
                    return new PredictionResponseDto
                    {
                        Prediction = "Not Likely Response",
                        Probability = 0.0,
                        ConfidenceLevel = "Low",
                        BusinessReasons = new List<string> { result?.Error ?? "Prediction error occurred." }
                    };
                }

                return new PredictionResponseDto
                {
                    Prediction = result.Prediction,
                    Probability = result.Probability,
                    ConfidenceLevel = result.ConfidenceLevel,
                    BusinessReasons = result.BusinessReasons ?? new List<string>()
                };
            }
            catch (Exception e)
            {
                return new PredictionResponseDto
                {
                    Prediction = "Not Likely Response",
                    Probability = 0.0,
                    ConfidenceLevel = "Low",
                    BusinessReasons = new List<string> { $"Exception during prediction pipeline: {e.Message}" }
                };
            }
        }

        /// <summary>
        /// Reads model performance metrics from metrics JSON.
        /// </summary>
        public async Task<PredictionMetricsDto?> GetMetricsAsync()
        {
            try
            {
                string analyticsDir = Path.Combine(Directory.GetCurrentDirectory(), "Analytics");
                if (!Directory.Exists(analyticsDir))
                {
                    analyticsDir = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "Analytics");
                }

                string metricsPath = Path.Combine(analyticsDir, "model_metrics.json");
                if (!File.Exists(metricsPath))
                {
                    // Train the model on demand to produce metrics
                    bool trained = await TrainModelAsync();
                    if (!trained || !File.Exists(metricsPath))
                    {
                        return null;
                    }
                }

                string json = await File.ReadAllTextAsync(metricsPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<PredictionMetricsDto>(json, options);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class PythonResultHelper
        {
            public bool Success { get; set; }
            public string Error { get; set; } = string.Empty;
        }

        private class PredictionResultHelper
        {
            public bool Success { get; set; }
            public string Error { get; set; } = string.Empty;
            public string Prediction { get; set; } = string.Empty;
            public double Probability { get; set; }
            public string ConfidenceLevel { get; set; } = string.Empty;
            public List<string>? BusinessReasons { get; set; }
        }
    }
}
