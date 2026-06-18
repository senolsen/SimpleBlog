using Blog.Core.Helpers;
using GeneratorLicenceCode.Data;
using GeneratorLicenceCode.Entities;
using GeneratorLicenceCode.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeneratorLicenceCode.Controllers;

[Authorize]
public class LicenseController : Controller
{
    private readonly LicenceDbContext _context;

    public LicenseController(LicenceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status)
    {
        var query = _context.Licenses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(l =>
                l.Domain.ToLower().Contains(term) ||
                (l.CustomerName != null && l.CustomerName.ToLower().Contains(term)) ||
                (l.Notes != null && l.Notes.ToLower().Contains(term)));
        }

        if (status == "active")
            query = query.Where(l => l.IsActive);
        else if (status == "passive")
            query = query.Where(l => !l.IsActive);

        var licenses = await query
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(licenses);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new LicenseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LicenseFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var normalizedDomain = model.Domain.Trim().ToLower();

        if (await _context.Licenses.AnyAsync(l => l.Domain == normalizedDomain))
        {
            ModelState.AddModelError(nameof(model.Domain), "Bu domain için zaten bir lisans kaydı mevcut.");
            return View(model);
        }

        var license = new LicenseRecord
        {
            Domain = normalizedDomain,
            LicenseKey = SecurityHelper.EncryptDomain(normalizedDomain),
            CustomerName = model.CustomerName?.Trim(),
            Notes = model.Notes?.Trim(),
            IsActive = model.IsActive,
            CreatedBy = User.Identity?.Name,
            CreatedDate = DateTime.Now
        };

        _context.Licenses.Add(license);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lisans başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = license.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var license = await _context.Licenses.FindAsync(id);
        if (license == null)
            return NotFound();

        return View(MapToViewModel(license));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var license = await _context.Licenses.FindAsync(id);
        if (license == null)
            return NotFound();

        return View(MapToViewModel(license));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LicenseFormViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var license = await _context.Licenses.FindAsync(id);
        if (license == null)
            return NotFound();

        var normalizedDomain = model.Domain.Trim().ToLower();

        if (await _context.Licenses.AnyAsync(l => l.Domain == normalizedDomain && l.Id != id))
        {
            ModelState.AddModelError(nameof(model.Domain), "Bu domain başka bir lisans kaydında kullanılıyor.");
            return View(model);
        }

        var domainChanged = license.Domain != normalizedDomain;

        license.Domain = normalizedDomain;
        license.CustomerName = model.CustomerName?.Trim();
        license.Notes = model.Notes?.Trim();
        license.IsActive = model.IsActive;
        license.UpdatedDate = DateTime.Now;
        license.UpdatedBy = User.Identity?.Name;

        if (domainChanged)
            license.LicenseKey = SecurityHelper.EncryptDomain(normalizedDomain);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = domainChanged
            ? "Lisans güncellendi ve domain değişikliği nedeniyle anahtar yeniden üretildi."
            : "Lisans başarıyla güncellendi.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var license = await _context.Licenses.FindAsync(id);
        if (license == null)
            return NotFound();

        license.IsDeleted = true;
        license.UpdatedDate = DateTime.Now;
        license.UpdatedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lisans kaydı silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateKey(int id)
    {
        var license = await _context.Licenses.FindAsync(id);
        if (license == null)
            return NotFound();

        license.LicenseKey = SecurityHelper.EncryptDomain(license.Domain);
        license.UpdatedDate = DateTime.Now;
        license.UpdatedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lisans anahtarı yeniden üretildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private static LicenseFormViewModel MapToViewModel(LicenseRecord license) => new()
    {
        Id = license.Id,
        Domain = license.Domain,
        LicenseKey = license.LicenseKey,
        CustomerName = license.CustomerName,
        Notes = license.Notes,
        IsActive = license.IsActive,
        CreatedDate = license.CreatedDate,
        UpdatedDate = license.UpdatedDate,
        CreatedBy = license.CreatedBy,
        UpdatedBy = license.UpdatedBy
    };
}
