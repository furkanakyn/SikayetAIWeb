using SikayetAIWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace SikayetAIWeb.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı boş bırakılamaz.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Kullanıcı adı alanı boş bırakılamaz.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = null!;

        [Display(Name = "Yeni Şifre")]
        public string? Password { get; set; } 

        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Kullanıcı Türü alanı boş bırakılamaz.")]
        [Display(Name = "Kullanıcı Türü")]
        public UserType UserType { get; set; }

        [Display(Name = "Departman")]
        public int? DepartmentId { get; set; }
    }
}