using SikayetAIWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace SikayetAIWeb.ViewModels
{
    public class ComplaintResponseViewModel
    {
        [Required]
        public int ComplaintId { get; set; }

        [Required(ErrorMessage = "Yanıt içeriği boş bırakılamaz.")]
        [Display(Name = "Yanıt")]
        public string Content { get; set; } = null!; // Response modelindeki isimle eşleşmesi için 'Content' olarak güncellendi
    }
}