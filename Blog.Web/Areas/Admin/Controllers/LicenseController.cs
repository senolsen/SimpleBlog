using Blog.Core.Entities;
using Blog.Core.Helpers;
using Blog.Data.Context;
using Blog.Web.Areas.Admin.Models;
using Blog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class LicenseController : Controller
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly LicenseSettings _licenseSettings;

    public LicenseController(
        AppDbContext context,
        IMemoryCache cache,
        IOptions<LicenseSettings> licenseSettings)
    {
        _context = context;
        _cache = cache;
        _licenseSettings = licenseSettings.Value;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Activate()
    {
        return View(BuildActivateViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(LicenseActivateViewModel model)
    {
        var currentDomain = HttpContext.Request.Host.Host.ToLower();
        model.CurrentDomain = currentDomain;
        model.SupportEmail = _licenseSettings.SupportEmail;
        model.SupportUrl = _licenseSettings.SupportUrl;
        model.CompanyName = _licenseSettings.CompanyName;

        if (!ModelState.IsValid)
            return View(model);

        var licenseKey = model.LicenseKey.Trim();
        var decryptedDomain = SecurityHelper.DecryptDomain(licenseKey);

        if (string.IsNullOrEmpty(decryptedDomain))
        {
            ModelState.AddModelError(string.Empty, "Geçersiz lisans anahtarı. Lütfen size iletilen kodu kontrol edin.");
            return View(model);
        }

        if (decryptedDomain != currentDomain)
        {
            ModelState.AddModelError(string.Empty,
                $"Bu lisans anahtarı \"{decryptedDomain}\" alan adı için geçerlidir. Mevcut alan adınız: \"{currentDomain}\".");
            return View(model);
        }

        await SaveLicenseAsync(licenseKey);
        _cache.Remove("IsLicenseValid");

        TempData["SuccessMessage"] = "Lisans başarıyla etkinleştirildi. Yönetim paneline giriş yapabilirsiniz.";
        return RedirectToAction("Login", "Auth", new { area = "Admin" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var license = await _context.Licenses.FirstOrDefaultAsync();
        return View(license ?? new License());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            TempData["ErrorMessage"] = "Lisans kodu boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var currentDomain = HttpContext.Request.Host.Host.ToLower();
        var decryptedDomain = SecurityHelper.DecryptDomain(licenseKey.Trim());

        if (string.IsNullOrEmpty(decryptedDomain))
        {
            TempData["ErrorMessage"] = "Geçersiz lisans anahtarı.";
            return RedirectToAction(nameof(Index));
        }

        if (decryptedDomain != currentDomain)
        {
            TempData["ErrorMessage"] =
                $"Bu lisans \"{decryptedDomain}\" için geçerlidir. Mevcut alan adınız: \"{currentDomain}\".";
            return RedirectToAction(nameof(Index));
        }

        await SaveLicenseAsync(licenseKey.Trim());
        _cache.Remove("IsLicenseValid");

        TempData["SuccessMessage"] = "Lisans anahtarı başarıyla kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    private LicenseActivateViewModel BuildActivateViewModel() => new()
    {
        CurrentDomain = HttpContext.Request.Host.Host.ToLower(),
        SupportEmail = _licenseSettings.SupportEmail,
        SupportUrl = _licenseSettings.SupportUrl,
        CompanyName = _licenseSettings.CompanyName
    };

    private async Task SaveLicenseAsync(string licenseKey)
    {
        var license = await _context.Licenses.FirstOrDefaultAsync();

        if (license == null)
        {
            license = new License { Key = licenseKey, IsActive = true };
            _context.Licenses.Add(license);
        }
        else
        {
            license.Key = licenseKey;
            license.IsActive = true;
            license.UpdatedDate = DateTime.Now;
            _context.Update(license);
        }

        await _context.SaveChangesAsync();
    }
}
