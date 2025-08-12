using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SikayetAIWeb.Services
{
    public class CategoryPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryPredictionService> _logger;
        private const string FallbackCategory = "Genel";

        public CategoryPredictionService(
            HttpClient httpClient,
            ILogger<CategoryPredictionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Flask API endpoint
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:5000/");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<List<string>> PredictCategoriesAsync(string complaintText)
        {
            if (string.IsNullOrWhiteSpace(complaintText))
            {
                _logger.LogWarning("Boş şikayet metni için tahmin yapılamıyor");
                return new List<string> { FallbackCategory };
            }

            try
            {
                var requestData = new { text = complaintText };
                var response = await _httpClient.PostAsJsonAsync("predict", requestData);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API hatası: {response.StatusCode}");
                    return new List<string> { FallbackCategory };
                }

                var result = await response.Content.ReadFromJsonAsync<PredictionResult>();

                if (result?.Labels != null && result.Labels.Count > 0)
                {
                    return result.Labels.Take(2).ToList();
                }

                _logger.LogWarning("API'den geçerli bir kategori gelmedi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tahmin hatası");
            }

            return new List<string> { FallbackCategory };
        }

        public async Task<string> PredictCategoryAsync(string complaintText)
        {
            var categories = await PredictCategoriesAsync(complaintText);
            return categories.FirstOrDefault() ?? FallbackCategory;
        }

        private class PredictionResult
        {
            public List<string> Labels { get; set; }
        }
    }
}
