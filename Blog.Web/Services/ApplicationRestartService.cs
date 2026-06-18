using Blog.Web.Models;

namespace Blog.Web.Services;

public interface IApplicationRestartService
{
    Task<RestartTriggerResult> TriggerRestartAsync(FtpConnectionInfo? ftpConnection, CancellationToken cancellationToken = default);
    string GetDetectedPlatformLabel();
}

public class ApplicationRestartService : IApplicationRestartService
{
    private readonly IFtpDeployService _ftpDeployService;
    private readonly UpdateSettings _updateSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApplicationRestartService> _logger;

    public ApplicationRestartService(
        IFtpDeployService ftpDeployService,
        Microsoft.Extensions.Options.IOptions<UpdateSettings> updateSettings,
        IWebHostEnvironment environment,
        ILogger<ApplicationRestartService> logger)
    {
        _ftpDeployService = ftpDeployService;
        _updateSettings = updateSettings.Value;
        _environment = environment;
        _logger = logger;
    }

    public string GetDetectedPlatformLabel()
    {
        if (OperatingSystem.IsWindows())
            return "Windows (IIS)";
        if (OperatingSystem.IsLinux())
            return "Linux";
        if (OperatingSystem.IsMacOS())
            return "macOS";
        return "Bilinmeyen";
    }

    public async Task<RestartTriggerResult> TriggerRestartAsync(
        FtpConnectionInfo? ftpConnection,
        CancellationToken cancellationToken = default)
    {
        var actions = new List<string>();
        var triggered = false;
        var strategy = _updateSettings.RestartStrategy ?? RestartStrategyNames.Auto;

        if (strategy is RestartStrategyNames.Auto or RestartStrategyNames.IisWebConfig)
        {
            if (ftpConnection != null && await TryFtpWebConfigTouchAsync(ftpConnection, actions, cancellationToken))
                triggered = true;

            if (TryLocalWebConfigTouch(actions))
                triggered = true;
        }

        if (strategy is RestartStrategyNames.Auto or RestartStrategyNames.LinuxTouchFile)
        {
            var touchFile = _updateSettings.LinuxRestartTouchFile;
            if (ftpConnection != null && await TryFtpRestartFileTouchAsync(ftpConnection, touchFile, actions, cancellationToken))
                triggered = true;

            if (TryLocalRestartFileTouch(touchFile, actions))
                triggered = true;
        }

        if (!triggered)
            actions.Add("Otomatik yeniden başlatma tetiklenemedi. Hosting panelinden uygulamayı yeniden başlatın.");

        return new RestartTriggerResult
        {
            Triggered = triggered,
            DetectedPlatform = GetDetectedPlatformLabel(),
            Actions = actions
        };
    }

    private async Task<bool> TryFtpWebConfigTouchAsync(
        FtpConnectionInfo connection,
        List<string> actions,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await _ftpDeployService.TouchWebConfigAsync(connection, cancellationToken))
            {
                actions.Add("FTP: web.config güncellendi (Windows/IIS restart).");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FTP web.config touch başarısız.");
        }

        return false;
    }

    private async Task<bool> TryFtpRestartFileTouchAsync(
        FtpConnectionInfo connection,
        string relativePath,
        List<string> actions,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await _ftpDeployService.TouchRestartFileAsync(connection, relativePath, cancellationToken))
            {
                actions.Add($"FTP: {relativePath} güncellendi (Linux restart).");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FTP restart dosyası touch başarısız.");
        }

        return false;
    }

    private bool TryLocalWebConfigTouch(List<string> actions)
    {
        try
        {
            var webConfigPath = Path.Combine(_environment.ContentRootPath, "web.config");
            if (!File.Exists(webConfigPath))
                return false;

            var content = File.ReadAllText(webConfigPath);
            var timestamp = DateTime.UtcNow.ToString("O");
            var comment = $"<!-- update-trigger:{timestamp} -->";

            if (content.Contains("<!-- update-trigger:", StringComparison.Ordinal))
            {
                var start = content.IndexOf("<!-- update-trigger:", StringComparison.Ordinal);
                var end = content.IndexOf("-->", start, StringComparison.Ordinal) + 3;
                content = content.Remove(start, end - start).Insert(start, comment);
            }
            else
            {
                content = content.TrimEnd() + Environment.NewLine + comment + Environment.NewLine;
            }

            File.WriteAllText(webConfigPath, content);
            actions.Add("Yerel: web.config güncellendi (Windows/IIS restart).");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Yerel web.config touch başarısız.");
            return false;
        }
    }

    private bool TryLocalRestartFileTouch(string relativePath, List<string> actions)
    {
        try
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(fullPath, DateTime.UtcNow.ToString("O"));
            actions.Add($"Yerel: {relativePath} güncellendi (Linux restart).");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Yerel restart dosyası touch başarısız.");
            return false;
        }
    }
}
