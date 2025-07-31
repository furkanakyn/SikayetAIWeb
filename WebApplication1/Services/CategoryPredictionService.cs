using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SikayetAIWeb.Services
{
    public class CategoryPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryPredictionService> _logger;
        private readonly IConfiguration _configuration;
        private const string FallbackCategory = "Genel";

        public CategoryPredictionService(
            HttpClient httpClient,
            ILogger<CategoryPredictionService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // Flask API adresini yapılandırmadan al
            var baseUrl = _configuration["FlaskApi:BaseUrl"] ?? "http://localhost:5000/";
            _httpClient.BaseAddress = new Uri(baseUrl);

            // Zaman aşımı süresi
            var timeoutSeconds = _configuration.GetValue<double>("FlaskApi:TimeoutSeconds", 15);
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        public async Task<string> PredictCategoryAsync(string complaintText)
        {
            Console.WriteLine("Tahmin için Flask API'ye gönderiliyor: " + complaintText);

            if (string.IsNullOrWhiteSpace(complaintText))
            {
                _logger.LogWarning("Boş şikayet metni için kategori tahmini yapılamıyor.");
                return FallbackCategory;
            }

            try
            {
                // Uzun metinleri kısaltma
                var truncatedText = complaintText.Length > 1000
                    ? complaintText.Substring(0, 1000)
                    : complaintText;

                var requestData = new { text = truncatedText };
                var endpoint = "predict"; // Flask endpoint

                var response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = await response.Content.ReadFromJsonAsync<PredictionResult>(options);

                    // Flask API'nin dönüş formatına göre ayarlayın
                    return result?.predicted_category ?? FallbackCategory;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API hatası ({response.StatusCode}): {errorContent}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API bağlantı hatası");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("API zaman aşımı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen tahmin hatası");
            }

            return FallbackCategory;
        }
    }

    public class PredictionResult
    {
        public string predicted_category { get; set; } = string.Empty;
    }
}