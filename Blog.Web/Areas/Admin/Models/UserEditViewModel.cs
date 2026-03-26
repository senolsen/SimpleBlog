using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad zorunludur.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = string.Empty;

    // Düzenleme ekranında şifre zorunlu değildir, boş bırakılırsa eski şifre kalır
    public string? Password { get; set; }

    [Required(ErrorMessage = "En az bir rol seçmelisiniz.")]
    public List<string> RoleNames { get; set; } = new List<string>();
}