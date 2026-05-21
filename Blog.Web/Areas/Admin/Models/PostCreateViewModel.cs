using Blog.Core.Enums;
using System.ComponentModel.DataAnnotations;
namespace Blog.Web.Areas.Admin.Models;

public class PostCreateViewModel
{
    [Required(ErrorMessage = "Başlık alanı zorunludur.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik alanı zorunludur.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    public int CategoryId { get; set; }

    public IFormFile? CoverImageFile { get; set; }

    // SEO Alanları
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public List<string> SelectedTags { get; set; } = new List<string>();

    public PostStatus Status { get; set; }
}