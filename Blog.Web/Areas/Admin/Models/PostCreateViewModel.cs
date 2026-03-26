using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class PostCreateViewModel
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Content { get; set; } = string.Empty;
    [Required] public int CategoryId { get; set; }
    public IFormFile? CoverImageFile { get; set; }

    [Display(Name = "SEO Başlığı (Meta Title)")]
    [MaxLength(70, ErrorMessage = "SEO başlığı en fazla 70 karakter olmalıdır.")]
    public string? MetaTitle { get; set; }

    [Display(Name = "SEO Açıklaması (Meta Description)")]
    [MaxLength(160, ErrorMessage = "SEO açıklaması en fazla 160 karakter olmalıdır.")]
    public string? MetaDescription { get; set; }
}