using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class ContactUpdateViewModel
{
    public int Id { get; set; }

    [Display(Name = "Telefon Numarası")]
    public string? Phone { get; set; }

    [Display(Name = "E-Posta Adresi")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string? Email { get; set; }

    [Display(Name = "Açık Adres")]
    public string? Address { get; set; }

    [Display(Name = "Google Harita (Iframe Src)")]
    public string? MapIframeSrc { get; set; }
}