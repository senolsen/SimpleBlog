using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Blog.Web.Areas.Admin.Models;

public class PostEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    public int CategoryId { get; set; }

    // Mevcut resmin yolunu arayüzde göstermek için
    public string? ExistingImagePath { get; set; }

    // Kullanıcı resmi değiştirmek isterse buraya dolacak
    public IFormFile? NewCoverImageFile { get; set; }

    [Display(Name = "SEO Başlığı (Meta Title)")]
    [MaxLength(70, ErrorMessage = "SEO başlığı en fazla 70 karakter olmalıdır.")]
    public string? MetaTitle { get; set; }

    [Display(Name = "SEO Açıklaması (Meta Description)")]
    [MaxLength(160, ErrorMessage = "SEO açıklaması en fazla 160 karakter olmalıdır.")]
    public string? MetaDescription { get; set; }
}