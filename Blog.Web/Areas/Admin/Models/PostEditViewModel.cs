using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Blog.Web.Areas.Admin.Models;

public class PostEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık alanı zorunludur.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik alanı zorunludur.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    public int CategoryId { get; set; }

    public string? ExistingImagePath { get; set; }
    public IFormFile? NewCoverImageFile { get; set; }

    // SEO Alanları
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}