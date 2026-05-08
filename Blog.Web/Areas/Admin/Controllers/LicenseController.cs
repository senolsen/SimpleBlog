using Blog.Core.Entities;
using Blog.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")] // Sadece tam yetkili patron girebilir
public class LicenseController : Controller
{
    private readonly AppDbContext _context;

    public LicenseController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var license = await _context.Licenses.FirstOrDefaultAsync();
        return View(license ?? new License());
    }

    [HttpPost]
    public async Task<IActionResult> Update(string licenseKey)
    {
        if (string.IsNullOrEmpty(licenseKey))
        {
            TempData["ErrorMessage"] = "Lisans kodu boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var license = await _context.Licenses.FirstOrDefaultAsync();

        if (license == null)
        {
            license = new License { Key = licenseKey.Trim(), IsActive = true };
            _context.Licenses.Add(license);
        }
        else
        {
            license.Key = licenseKey.Trim();
            license.UpdatedDate = DateTime.Now;
            _context.Update(license);
        }

        await _context.SaveChangesAsync();

        // ÖNEMLİ: Lisans güncellendiği için Cache'i temizlemeliyiz ki Middleware yeni kodu görsün!
        // (MemoryCache nesnesini buraya inject edip .Remove("IsLicenseValid") yapmalısın)

        TempData["SuccessMessage"] = "Lisans anahtarı başarıyla kaydedildi. Sistem kontrol ediliyor...";
        return RedirectToAction(nameof(Index));
    }
}