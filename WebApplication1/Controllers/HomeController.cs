using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                dynamic result = JsonConvert.DeserializeObject(json);

                ViewBag.Prediction = result?.label ?? "belirlenemedi";
                ViewBag.Confidence = result?.confidence ?? 0;
                ViewBag.OriginalText = text;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Hata oluştu: {ex.Message}";
            }

            return View();
        }
    }
}