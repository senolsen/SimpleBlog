namespace Blog.Core.Entities;

public class SiteSetting : BaseEntity
{
    // Temel Ayarlar
    public string SiteTitle { get; set; } = string.Empty;
    public string? SiteDescription { get; set; }
    public string? LogoPath { get; set; }
    public string? FaviconPath { get; set; }

    // İletişim Bilgileri
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactAddress { get; set; }

    // Sosyal Medya
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? LinkedinUrl { get; set; }

    // Gelişmiş Ayarlar (SEO/Analytics vb.)
    public string? GoogleAnalyticsCode { get; set; }

    // İletişim Bilgileri (Öncekilerin altına ekle)
    public string? MapUrl { get; set; } // Google Maps Iframe kodu veya linki
    public string? WorkingHours { get; set; } // Örn: Pzt-Cuma 09:00 - 18:00
}