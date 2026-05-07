using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class TagEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Etiket adı zorunludur.")]
    [MaxLength(50, ErrorMessage = "Etiket en fazla 50 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;
}