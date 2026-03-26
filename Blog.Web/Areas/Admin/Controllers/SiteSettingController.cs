using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")] // Bu ayarları sadece Admin değiştirebilmeli
public class SiteSettingController : Controller
{
    private readonly IGenericService<SiteSetting> _settingService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SiteSettingController(IGenericService<SiteSetting> settingService, IWebHostEnvironment webHostEnvironment)
    {
        _settingService = settingService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Tablodaki ilk kaydı getir, yoksa yeni oluştur (Singleton mantığı)
        var settingsList = await _settingService.GetAllAsync();
        var setting = settingsList.FirstOrDefault();

        if (setting == null)
        {
            setting = new SiteSetting
            {
                SiteTitle = "Tetraboss.com - Blog",
                ContactAddress = "Sancaktepe, İstanbul",
                ContactEmail = "info@tetraboss.com"
            };
            await _settingService.AddAsync(setting);
        }

        var model = new SiteSettingUpdateViewModel
        {
            Id = setting.Id,
            SiteTitle = setting.SiteTitle,
            SiteDescription = setting.SiteDescription,
            ExistingLogoPath = setting.LogoPath,
            ExistingFaviconPath = setting.FaviconPath,
            ContactEmail = setting.ContactEmail,
            ContactPhone = setting.ContactPhone,
            ContactAddress = setting.ContactAddress,
            FacebookUrl = setting.FacebookUrl,
            InstagramUrl = setting.InstagramUrl,
            GithubUrl = setting.GithubUrl,
            LinkedinUrl = setting.LinkedinUrl,
            GoogleAnalyticsCode = setting.GoogleAnalyticsCode
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSettingUpdateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var setting = await _settingService.GetByIdAsync(model.Id);
            if (setting == null) return NotFound();

            // Dosya Yükleme (Logo)
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                var extension = Path.GetExtension(model.LogoFile.FileName).ToLowerInvariant();
                string uniqueFileName = "logo_" + Guid.NewGuid().ToString() + extension;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "settings");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await model.LogoFile.CopyToAsync(fileStream); }

                // Eski logoyu silme işlemi buraya eklenebilir
                setting.LogoPath = "/uploads/settings/" + uniqueFileName;
            }

            // Temel Verileri Güncelle
            setting.SiteTitle = model.SiteTitle;
            setting.SiteDescription = model.SiteDescription;
            setting.ContactEmail = model.ContactEmail;
            setting.ContactPhone = model.ContactPhone;
            setting.ContactAddress = model.ContactAddress;
            setting.FacebookUrl = model.FacebookUrl;
            setting.InstagramUrl = model.InstagramUrl;
            setting.GithubUrl = model.GithubUrl;
            setting.LinkedinUrl = model.LinkedinUrl;
            setting.GoogleAnalyticsCode = model.GoogleAnalyticsCode;

            await _settingService.UpdateAsync(setting);
            TempData["SuccessMessage"] = "Site ayarları başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }
}