using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class LicenseActivateViewModel
{
    [Required(ErrorMessage = "Lisans anahtarı boş olamaz.")]
    [Display(Name = "Lisans Anahtarı")]
    public string LicenseKey { get; set; } = string.Empty;

    public string CurrentDomain { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportUrl { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}
