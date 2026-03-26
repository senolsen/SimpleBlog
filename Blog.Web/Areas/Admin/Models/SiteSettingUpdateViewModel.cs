using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Blog.Web.Areas.Admin.Models;

public class SiteSettingUpdateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Site başlığı boş bırakılamaz.")]
    public string SiteTitle { get; set; } = string.Empty;
    public string? SiteDescription { get; set; }

    // Resim Yüklemeleri İçin
    public IFormFile? LogoFile { get; set; }
    public string? ExistingLogoPath { get; set; }

    public IFormFile? FaviconFile { get; set; }
    public string? ExistingFaviconPath { get; set; }

    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactAddress { get; set; }

    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? LinkedinUrl { get; set; }

    public string? GoogleAnalyticsCode { get; set; }

    public string? MapUrl { get; set; }
    public string? WorkingHours { get; set; }
}