using System.ComponentModel.DataAnnotations;
using SikayetAIWeb.Models;

namespace SikayetAIWeb.ViewModels
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "Şifreniz en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Kullanıcı tipi zorunludur.")]
        [Display(Name = "Kullanıcı Tipi")]
        public UserType UserType { get; set; } = UserType.citizen;

        [Display(Name = "Departman ID")]
        public int? DepartmentId { get; set; }
    }
}