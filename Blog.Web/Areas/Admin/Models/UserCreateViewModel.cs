using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class UserCreateViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;

    // ÇOKLU ROL SEÇİMİ İÇİN LİSTE (Artık ID değil İsim tutuyoruz)
    [Required(ErrorMessage = "En az bir rol seçmelisiniz.")]
    public List<string> RoleNames { get; set; } = new List<string>();
}