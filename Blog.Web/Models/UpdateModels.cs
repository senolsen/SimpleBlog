namespace Blog.Web.Models;

public class UpdateSettings
{
    public bool Enabled { get; set; } = true;
    public string GitHubOwner { get; set; } = string.Empty;
    public string GitHubRepo { get; set; } = string.Empty;
    public string? GitHubToken { get; set; }
    public string AssetFileNamePattern { get; set; } = ".zip";
    public string RestartStrategy { get; set; } = RestartStrategyNames.Auto;
    public string LinuxRestartTouchFile { get; set; } = "tmp/restart.txt";
}

public class UpdateCheckResult
{
    public bool IsConfigured { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public string? LatestVersion { get; set; }
    public bool UpdateAvailable { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ReleasePageUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UpdateInstallResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool RequiresRestart { get; set; }
}

public record FtpConnectionInfo(
    string Host,
    int Port,
    string Username,
    string Password,
    string RemotePath,
    bool UseSsl);
