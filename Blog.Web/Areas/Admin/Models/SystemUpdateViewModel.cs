using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class SystemUpdateViewModel
{
    public UpdateInfoViewModel UpdateInfo { get; set; } = new();
    public FtpSettingsViewModel FtpSettings { get; set; } = new();
    public bool HasSavedFtpPassword { get; set; }
}

public class UpdateInfoViewModel
{
    public bool IsConfigured { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public string? LatestVersion { get; set; }
    public bool UpdateAvailable { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? ReleasePageUrl { get; set; }
    public string? CheckError { get; set; }
    public string HostingPlatform { get; set; } = string.Empty;
}

public class FtpSettingsViewModel
{
    [Display(Name = "FTP Sunucu")]
    public string? Host { get; set; }

    [Display(Name = "Port")]
    [Range(1, 65535)]
    public int Port { get; set; } = 21;

    [Display(Name = "Kullanıcı Adı")]
    public string? Username { get; set; }

    [Display(Name = "Şifre")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Uzak Klasör")]
    public string RemotePath { get; set; } = "/";

    [Display(Name = "FTPS (SSL) Kullan")]
    public bool UseSsl { get; set; }
}
