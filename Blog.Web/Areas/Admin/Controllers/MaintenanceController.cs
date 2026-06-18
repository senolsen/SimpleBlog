using Blog.Data.Context;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Blog.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MaintenanceController : Controller
{
    private readonly AppDbContext _context;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IDataSeeder _dataSeeder;

    public MaintenanceController(
        AppDbContext context,
        ISiteSettingsService siteSettingsService,
        IDataSeeder dataSeeder)
    {
        _context = context;
        _siteSettingsService = siteSettingsService;
        _dataSeeder = dataSeeder;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var setting = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync()
                      ?? new Blog.Core.Entities.SiteSetting();

        var model = new MaintenanceViewModel
        {
            IsMaintenanceMode = setting.IsMaintenanceMode,
            MaintenanceMessage = setting.MaintenanceMessage,
            SiteTitle = setting.SiteTitle
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMode(MaintenanceViewModel model)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new Blog.Core.Entities.SiteSetting { SiteTitle = "SimpleBlog" };
            _context.SiteSettings.Add(setting);
        }

        setting.IsMaintenanceMode = Request.Form["IsMaintenanceMode"].Contains("true");
        setting.MaintenanceMessage = string.IsNullOrWhiteSpace(model.MaintenanceMessage)
            ? null
            : model.MaintenanceMessage.Trim();
        setting.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        _siteSettingsService.InvalidateCache();

        TempData["SuccessMessage"] = setting.IsMaintenanceMode
            ? "Bakım modu etkinleştirildi. Ziyaretçiler bakım sayfasını görecek."
            : "Bakım modu kapatıldı. Site tekrar yayında.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(string confirmationText)
    {
        if (confirmationText?.Trim().ToUpperInvariant() != "SIFIRLA")
        {
            TempData["ErrorMessage"] = "Onay metni hatalı. Devam etmek için kutucuğa SIFIRLA yazın.";
            return RedirectToAction(nameof(Index));
        }

        await _dataSeeder.ResetToDemoAsync();

        TempData["SuccessMessage"] = "Demo veriler başarıyla sıfırlandı. Site varsayılan içerikle yeniden oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }
}
