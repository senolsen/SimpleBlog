using Blog.Web.Areas.Admin.Models;
using Blog.Web.Models;
using Blog.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SystemUpdateController : Controller
{
    private readonly IUpdateService _updateService;

    public SystemUpdateController(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFtp(FtpSettingsViewModel ftpSettings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ftpSettings.Host)
            || string.IsNullOrWhiteSpace(ftpSettings.Username))
        {
            TempData["ErrorMessage"] = "FTP sunucu ve kullanıcı adı zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        var existingConnection = await _updateService.GetFtpConnectionAsync();
        if (string.IsNullOrWhiteSpace(ftpSettings.Password) && existingConnection == null)
        {
            TempData["ErrorMessage"] = "İlk kayıt için FTP şifresi zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        var connection = new FtpConnectionInfo(
            ftpSettings.Host!.Trim(),
            ftpSettings.Port <= 0 ? 21 : ftpSettings.Port,
            ftpSettings.Username!.Trim(),
            ftpSettings.Password ?? existingConnection?.Password ?? string.Empty,
            string.IsNullOrWhiteSpace(ftpSettings.RemotePath) ? "/" : ftpSettings.RemotePath.Trim(),
            ftpSettings.UseSsl);

        await _updateService.SaveFtpSettingsAsync(connection, ftpSettings.Password, clearPassword: false);

        TempData["SuccessMessage"] = "FTP ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestFtp(CancellationToken cancellationToken)
    {
        try
        {
            await _updateService.TestFtpConnectionAsync(cancellationToken);
            TempData["SuccessMessage"] = "FTP bağlantısı başarılı.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"FTP bağlantısı başarısız: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Install(CancellationToken cancellationToken)
    {
        var result = await _updateService.InstallLatestAsync(cancellationToken);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    private async Task<SystemUpdateViewModel> BuildViewModelAsync(CancellationToken cancellationToken)
    {
        var check = await _updateService.CheckForUpdateAsync(cancellationToken);
        var ftpConnection = await _updateService.GetFtpConnectionAsync();

        return new SystemUpdateViewModel
        {
            UpdateInfo = new UpdateInfoViewModel
            {
                IsConfigured = check.IsConfigured,
                CurrentVersion = check.CurrentVersion,
                LatestVersion = check.LatestVersion,
                UpdateAvailable = check.UpdateAvailable,
                ReleaseNotes = check.ReleaseNotes,
                ReleasePageUrl = check.ReleasePageUrl,
                CheckError = check.ErrorMessage,
                HostingPlatform = _updateService.GetHostingPlatformLabel()
            },
            HasSavedFtpPassword = ftpConnection != null,
            FtpSettings = new FtpSettingsViewModel
            {
                Host = ftpConnection?.Host,
                Port = ftpConnection?.Port ?? 21,
                Username = ftpConnection?.Username,
                RemotePath = ftpConnection?.RemotePath ?? "/",
                UseSsl = ftpConnection?.UseSsl ?? false
            }
        };
    }
}
