using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class CategoryEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    // Entity'de olan gerçek alanlar
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}