using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Services;
using System.Threading.Tasks;

namespace SikayetAIWeb.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryApiController : ControllerBase
    {
        private readonly CategoryPredictionService _categoryService;

        public CategoryApiController(CategoryPredictionService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromBody] PredictionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Text))
                return BadRequest(new { error = "Metin boş olamaz." });

            var labels = await _categoryService.PredictCategoriesAsync(req.Text);

           
            return Ok(new { labels });
        }

        public class PredictionRequest
        {
            public string Text { get; set; }
        }
    }
}
