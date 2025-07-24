using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SikayetAIWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5000");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public class PredictionResponse
        {
            public List<string> labels { get; set; }
            public List<float> confidences { get; set; }
            public float latency_ms { get; set; }
            public string status { get; set; }
            public string message { get; set; } // hata mesajı olabilir
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string text)
        {
            try
            {
                var payload = new { text };
                var response = await _httpClient.PostAsJsonAsync("/predict", payload);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PredictionResponse>(json);

                if (result.status == "success")
                {
                    ViewBag.Predictions = result.labels;
                    ViewBag.Confidences = result.confidences;
                    ViewBag.Latency = result.latency_ms;
                }
                else
                {
                    ViewBag.Error = result.message ?? "Bilinmeyen bir hata oluştu.";
                }
                ViewBag.OriginalText = text;
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = $"Hata oluştu: {ex.Message}";
            }

            return View();
        }
    }
}
