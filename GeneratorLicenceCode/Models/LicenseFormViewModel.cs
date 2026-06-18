using System.ComponentModel.DataAnnotations;

namespace GeneratorLicenceCode.Models;

public class LicenseFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Domain alanı zorunludur.")]
    [Display(Name = "Domain")]
    [RegularExpression(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$",
        ErrorMessage = "Geçerli bir domain girin (örn: musteri.com).")]
    public string Domain { get; set; } = string.Empty;

    [Display(Name = "Müşteri Adı")]
    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [Display(Name = "Notlar")]
    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Lisans Anahtarı")]
    public string? LicenseKey { get; set; }

    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
