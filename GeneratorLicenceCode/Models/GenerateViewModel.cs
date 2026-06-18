using System.ComponentModel.DataAnnotations;

namespace GeneratorLicenceCode.Models;

public class GenerateViewModel
{
    [Required(ErrorMessage = "Domain alanı zorunludur.")]
    [Display(Name = "Lisanslanacak Domain")]
    [RegularExpression(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$",
        ErrorMessage = "Geçerli bir domain girin (örn: musteri.com).")]
    public string Domain { get; set; } = string.Empty;

    public string? GeneratedKey { get; set; }
    public string? VerifiedDomain { get; set; }
    public bool? IsVerified { get; set; }
    public string? ErrorMessage { get; set; }
}
