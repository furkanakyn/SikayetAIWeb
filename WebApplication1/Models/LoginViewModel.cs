// Models/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SikayetAIWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gerekli.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre gerekli.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}