using Microsoft.AspNetCore.Mvc;
using SikayetAIWeb.Models;
using System.Diagnostics;

namespace SikayetAIWeb.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index()
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }
    }
}
