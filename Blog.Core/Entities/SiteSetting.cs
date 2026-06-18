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

    public string? AdsenseCode { get; set; } // Ana AdSense scripti
    public string? SidebarAdCode { get; set; } // Yan menü reklam alanı
    public string? PostBottomAdCode { get; set; } // Yazı altı reklam alanı
    public string? HomeListAdCode { get; set; } // Ana sayfa liste arası reklam

    public string? AdsTxtContent { get; set; }

    public string? RobotsTxtContent { get; set; }

    public string ActiveTheme { get; set; } = "MagDesign";

    // Bakım Modu
    public bool IsMaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }

    // Sistem Güncelleme (FTP)
    public string? UpdateFtpHost { get; set; }
    public int UpdateFtpPort { get; set; } = 21;
    public string? UpdateFtpUsername { get; set; }
    public string? UpdateFtpPasswordProtected { get; set; }
    public string? UpdateFtpRemotePath { get; set; } = "/";
    public bool UpdateFtpUseSsl { get; set; }
}