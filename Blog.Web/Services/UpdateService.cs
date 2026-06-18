using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Blog.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blog.Web.Services;

public partial class UpdateService : IUpdateService
{
    private static readonly string[] PreservedRelativePaths =
    [
        "appsettings.Production.json",
        "appsettings.Development.json",
        "App_Data",
        "wwwroot/uploads"
    ];

    private readonly UpdateSettings _updateSettings;
    private readonly AppDbContext _context;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IFtpDeployService _ftpDeployService;
    private readonly IApplicationRestartService _restartService;
    private readonly IFtpCredentialProtector _credentialProtector;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(
        IOptions<UpdateSettings> updateSettings,
        AppDbContext context,
        ISiteSettingsService siteSettingsService,
        IFtpDeployService ftpDeployService,
        IApplicationRestartService restartService,
        IFtpCredentialProtector credentialProtector,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<UpdateService> logger)
    {
        _updateSettings = updateSettings.Value;
        _context = context;
        _siteSettingsService = siteSettingsService;
        _ftpDeployService = ftpDeployService;
        _restartService = restartService;
        _credentialProtector = credentialProtector;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? assembly.GetName().Version?.ToString()
               ?? "1.0.0";
    }

    public async Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        var current = NormalizeVersion(GetCurrentVersion());

        if (!_updateSettings.Enabled
            || string.IsNullOrWhiteSpace(_updateSettings.GitHubOwner)
            || string.IsNullOrWhiteSpace(_updateSettings.GitHubRepo))
        {
            return new UpdateCheckResult
            {
                IsConfigured = false,
                CurrentVersion = current,
                ErrorMessage = "GitHub güncelleme ayarları yapılandırılmamış."
            };
        }

        try
        {
            var client = _httpClientFactory.CreateClient("GitHubUpdates");
            var url = $"https://api.github.com/repos/{_updateSettings.GitHubOwner}/{_updateSettings.GitHubRepo}/releases/latest";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyGitHubAuth(request);
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;

            var tag = root.GetProperty("tag_name").GetString() ?? string.Empty;
            var latest = NormalizeVersion(tag);
            var notes = root.TryGetProperty("body", out var bodyElement) ? bodyElement.GetString() : null;
            var htmlUrl = root.TryGetProperty("html_url", out var htmlElement) ? htmlElement.GetString() : null;

            string? downloadUrl = null;
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? string.Empty;
                    if (name.Contains(_updateSettings.AssetFileNamePattern, StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
            }

            return new UpdateCheckResult
            {
                IsConfigured = true,
                CurrentVersion = current,
                LatestVersion = latest,
                UpdateAvailable = IsNewerVersion(latest, current) && !string.IsNullOrEmpty(downloadUrl),
                ReleaseNotes = notes,
                DownloadUrl = downloadUrl,
                ReleasePageUrl = htmlUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güncelleme kontrolü başarısız.");
            return new UpdateCheckResult
            {
                IsConfigured = true,
                CurrentVersion = current,
                ErrorMessage = "Güncelleme sunucusuna bağlanılamadı. Daha sonra tekrar deneyin."
            };
        }
    }

    public async Task<UpdateInstallResult> InstallLatestAsync(CancellationToken cancellationToken = default)
    {
        var check = await CheckForUpdateAsync(cancellationToken);
        if (!check.UpdateAvailable || string.IsNullOrEmpty(check.DownloadUrl))
        {
            return new UpdateInstallResult
            {
                Success = false,
                Message = check.ErrorMessage ?? "Kurulacak yeni sürüm bulunamadı."
            };
        }

        var ftpConnection = await GetFtpConnectionAsync();
        if (ftpConnection == null)
        {
            return new UpdateInstallResult
            {
                Success = false,
                Message = "FTP ayarları eksik. Güncelleme kurulumu için FTP bilgilerini kaydedin."
            };
        }

        var stagingRoot = Path.Combine(_environment.ContentRootPath, "App_Data", "update-staging");
        var downloadPath = Path.Combine(stagingRoot, "release.zip");
        var extractPath = Path.Combine(stagingRoot, "extracted");

        try
        {
            await EnableMaintenanceModeAsync(cancellationToken);

            Directory.CreateDirectory(stagingRoot);
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            await DownloadFileAsync(check.DownloadUrl, downloadPath, cancellationToken);
            ZipFile.ExtractToDirectory(downloadPath, extractPath);

            var sourceRoot = ResolveExtractedRoot(extractPath);
            var filesToDeploy = GetDeployableFiles(sourceRoot);

            if (filesToDeploy.Count == 0)
            {
                return new UpdateInstallResult
                {
                    Success = false,
                    Message = "Güncelleme paketinde kurulacak dosya bulunamadı."
                };
            }

            await _ftpDeployService.UploadDirectoryAsync(ftpConnection, sourceRoot, filesToDeploy, cancellationToken);

            var restart = await _restartService.TriggerRestartAsync(ftpConnection, cancellationToken);

            var message = restart.Triggered
                ? $"Sürüm {check.LatestVersion} başarıyla kuruldu. Site birkaç saniye içinde yeniden başlayacak. " +
                  "Yeniden bağlandıktan sonra bakım modunu kapatabilirsiniz."
                : $"Sürüm {check.LatestVersion} dosyaları yüklendi. Otomatik yeniden başlatma tetiklenemedi; " +
                  "hosting panelinden uygulamayı yeniden başlatın, ardından bakım modunu kapatın.";

            return new UpdateInstallResult
            {
                Success = true,
                RequiresRestart = true,
                Message = message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güncelleme kurulumu başarısız.");
            return new UpdateInstallResult
            {
                Success = false,
                Message = $"Güncelleme kurulumu başarısız: {ex.Message}"
            };
        }
        finally
        {
            TryCleanup(stagingRoot);
        }
    }

    public async Task<FtpConnectionInfo?> GetFtpConnectionAsync()
    {
        var setting = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
        if (setting == null
            || string.IsNullOrWhiteSpace(setting.UpdateFtpHost)
            || string.IsNullOrWhiteSpace(setting.UpdateFtpUsername)
            || string.IsNullOrWhiteSpace(setting.UpdateFtpPasswordProtected))
        {
            return null;
        }

        return new FtpConnectionInfo(
            setting.UpdateFtpHost.Trim(),
            setting.UpdateFtpPort <= 0 ? 21 : setting.UpdateFtpPort,
            setting.UpdateFtpUsername.Trim(),
            _credentialProtector.Unprotect(setting.UpdateFtpPasswordProtected),
            string.IsNullOrWhiteSpace(setting.UpdateFtpRemotePath) ? "/" : setting.UpdateFtpRemotePath.Trim(),
            setting.UpdateFtpUseSsl);
    }

    public async Task SaveFtpSettingsAsync(FtpConnectionInfo? connection, string? newPassword, bool clearPassword)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync()
                      ?? new SiteSetting { SiteTitle = "SimpleBlog" };

        if (setting.Id == 0)
            _context.SiteSettings.Add(setting);

        if (connection == null)
        {
            setting.UpdateFtpHost = null;
            setting.UpdateFtpPort = 21;
            setting.UpdateFtpUsername = null;
            setting.UpdateFtpPasswordProtected = null;
            setting.UpdateFtpRemotePath = "/";
            setting.UpdateFtpUseSsl = false;
        }
        else
        {
            setting.UpdateFtpHost = connection.Host;
            setting.UpdateFtpPort = connection.Port;
            setting.UpdateFtpUsername = connection.Username;
            setting.UpdateFtpRemotePath = connection.RemotePath;
            setting.UpdateFtpUseSsl = connection.UseSsl;

            if (clearPassword)
                setting.UpdateFtpPasswordProtected = null;
            else if (!string.IsNullOrWhiteSpace(newPassword))
                setting.UpdateFtpPasswordProtected = _credentialProtector.Protect(newPassword.Trim());
        }

        setting.UpdatedDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task TestFtpConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = await GetFtpConnectionAsync()
                         ?? throw new InvalidOperationException("FTP ayarları eksik.");

        await _ftpDeployService.TestConnectionAsync(connection, cancellationToken);
    }

    public string GetHostingPlatformLabel() => _restartService.GetDetectedPlatformLabel();

    private async Task EnableMaintenanceModeAsync(CancellationToken cancellationToken)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);
        if (setting == null)
            return;

        setting.IsMaintenanceMode = true;
        setting.MaintenanceMessage ??= "Sistem güncelleniyor. Kısa süre içinde tekrar yayında olacağız.";
        setting.UpdatedDate = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);
        _siteSettingsService.InvalidateCache();
    }

    private async Task DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("GitHubUpdates");
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyGitHubAuth(request);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = File.Create(destinationPath);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    private static string ResolveExtractedRoot(string extractPath)
    {
        var directWebConfig = Path.Combine(extractPath, "web.config");
        var directDll = Path.Combine(extractPath, "Blog.Web.dll");
        if (File.Exists(directWebConfig) || File.Exists(directDll))
            return extractPath;

        var subDirs = Directory.GetDirectories(extractPath);
        if (subDirs.Length == 1)
        {
            var candidate = subDirs[0];
            if (File.Exists(Path.Combine(candidate, "web.config")) || File.Exists(Path.Combine(candidate, "Blog.Web.dll")))
                return candidate;
        }

        return extractPath;
    }

    private static List<string> GetDeployableFiles(string sourceRoot)
    {
        var files = new List<string>();

        foreach (var absolutePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, absolutePath).Replace('\\', '/');

            if (ShouldPreserve(relative))
                continue;

            files.Add(relative);
        }

        return files;
    }

    private static bool ShouldPreserve(string relativePath)
    {
        foreach (var preserved in PreservedRelativePaths)
        {
            if (relativePath.Equals(preserved, StringComparison.OrdinalIgnoreCase)
                || relativePath.StartsWith(preserved + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void TryCleanup(string stagingRoot)
    {
        try
        {
            if (Directory.Exists(stagingRoot))
                Directory.Delete(stagingRoot, true);
        }
        catch
        {
            // Restart sırasında dosya kilitlenebilir; sessizce geç.
        }
    }

    private static string NormalizeVersion(string version)
    {
        var cleaned = VersionPrefixRegex().Replace(version.Trim(), string.Empty);
        var plusIndex = cleaned.IndexOf('+');
        if (plusIndex >= 0)
            cleaned = cleaned[..plusIndex];

        return cleaned;
    }

    private static bool IsNewerVersion(string latest, string current)
    {
        if (Version.TryParse(latest, out var latestVersion) && Version.TryParse(current, out var currentVersion))
            return latestVersion > currentVersion;

        return !string.Equals(latest, current, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"^v", RegexOptions.IgnoreCase)]
    private static partial Regex VersionPrefixRegex();

    private void ApplyGitHubAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_updateSettings.GitHubToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _updateSettings.GitHubToken);
    }
}
