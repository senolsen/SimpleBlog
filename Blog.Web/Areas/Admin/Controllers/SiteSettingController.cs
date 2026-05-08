using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SiteSettingController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public SiteSettingController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSetting();

        var model = new SiteSettingUpdateViewModel
        {
            Id = setting.Id,
            SiteTitle = setting.SiteTitle ?? string.Empty,
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
            GoogleAnalyticsCode = setting.GoogleAnalyticsCode,
            MapUrl = setting.MapUrl,
            WorkingHours = setting.WorkingHours,
            AdsenseCode = setting.AdsenseCode,
            SidebarAdCode = setting.SidebarAdCode,
            PostBottomAdCode = setting.PostBottomAdCode,
            AdsTxtContent=setting.AdsTxtContent
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(SiteSettingUpdateViewModel model)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new SiteSetting();
            _context.SiteSettings.Add(setting);
        }

        // --- Dosya Yükleme İşlemleri ---
        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "settings");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        if (model.LogoFile != null)
        {
            string logoName = "logo_" + Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
            string logoPath = Path.Combine(uploadsFolder, logoName);
            using (var fileStream = new FileStream(logoPath, FileMode.Create))
            {
                await model.LogoFile.CopyToAsync(fileStream);
            }
            setting.LogoPath = "/uploads/settings/" + logoName;
        }

        if (model.FaviconFile != null)
        {
            string faviconName = "favicon_" + Guid.NewGuid() + Path.GetExtension(model.FaviconFile.FileName);
            string faviconPath = Path.Combine(uploadsFolder, faviconName);
            using (var fileStream = new FileStream(faviconPath, FileMode.Create))
            {
                await model.FaviconFile.CopyToAsync(fileStream);
            }
            setting.FaviconPath = "/uploads/settings/" + faviconName;
        }

        // --- Diğer Verileri Kaydet ---
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
        setting.MapUrl = model.MapUrl;
        setting.WorkingHours = model.WorkingHours;
        setting.AdsenseCode = model.AdsenseCode;
        setting.SidebarAdCode = model.SidebarAdCode;
        setting.PostBottomAdCode = model.PostBottomAdCode;
        setting.AdsTxtContent = model.AdsTxtContent;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tüm ayarlar başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}