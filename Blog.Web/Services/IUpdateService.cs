using Blog.Web.Models;

namespace Blog.Web.Services;

public interface IUpdateService
{
    string GetCurrentVersion();
    Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default);
    Task<UpdateInstallResult> InstallLatestAsync(CancellationToken cancellationToken = default);
    Task<FtpConnectionInfo?> GetFtpConnectionAsync();
    Task SaveFtpSettingsAsync(FtpConnectionInfo? connection, string? newPassword, bool clearPassword);
    Task TestFtpConnectionAsync(CancellationToken cancellationToken = default);
    string GetHostingPlatformLabel();
}
