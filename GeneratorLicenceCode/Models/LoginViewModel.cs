using System.ComponentModel.DataAnnotations;

namespace GeneratorLicenceCode.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Güvenlik sorusunun cevabı zorunludur.")]
    [Display(Name = "Güvenlik Sorusu")]
    public int? MathAnswer { get; set; }
}
