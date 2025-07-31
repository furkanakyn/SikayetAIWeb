using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Services;
using System.Threading.Tasks;

namespace SikayetAIWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryPredictionController : ControllerBase
    {
        private readonly CategoryPredictionService _predictionService;

        public CategoryPredictionController(CategoryPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> PredictCategory([FromBody] PredictionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Metin boş olamaz");
            }

            var category = await _predictionService.PredictCategoryAsync(request.Text);
            return Ok(new { predictedCategory = category });
     
        
      }

    }

    public class PredictionRequest
    {
        public string Text { get; set; }
    }

}
