using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SikayetAIWeb.Models;

namespace SikayetAIWeb.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public ComplaintController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5000");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Text")] Complaint complaint)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Flask API'ye istek gönder
                    var response = await _httpClient.PostAsJsonAsync("/predict", new { text = complaint.Text });
                    Console.WriteLine("Gelen metin: " + complaint.Text);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadFromJsonAsync<PredictionResponse>();

                    // Veritabanına kaydet
                    complaint.Category = result?.Label ?? "diğer";
                    complaint.CreatedAt = DateTime.Now;

                    _context.Add(complaint);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(List));
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError("", $"API hatası: {ex.Message}");
                }
            }
            return View(complaint);
        }

        public async Task<IActionResult> List()
        {
            return View(await _context.Complaints.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }
    }

    public class PredictionResponse
    {
        public string Label { get; set; }
        public float Confidence { get; set; }
    }
}