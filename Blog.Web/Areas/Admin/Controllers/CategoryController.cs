using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly IGenericService<Category> _categoryService;

    public CategoryController(IGenericService<Category> categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            // SLUG OTOMATİK ÜRETİLİYOR
            category.Slug = UrlHelper.GenerateSlug(category.Name);

            await _categoryService.AddAsync(category);
            TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        // Kategori yoksa veya zaten silinmişse 404 sayfasına gönder
        if (category == null || category.IsDeleted) return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            var existingCategory = await _categoryService.GetByIdAsync(category.Id);
            if (existingCategory == null) return NotFound();

            // Kategori Adını Güncelle
            existingCategory.Name = category.Name;

            // YENİ EKLENEN SEO ALANLARINI GÜNCELLE
            existingCategory.MetaTitle = category.MetaTitle;
            existingCategory.MetaDescription = category.MetaDescription;

            // Kategori adı değişmiş olabileceği için Slug'ı yeniden üretiyoruz
            existingCategory.Slug = UrlHelper.GenerateSlug(category.Name);

            await _categoryService.UpdateAsync(existingCategory);

            // Başarı mesajını ekliyoruz
            TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // Soft Delete İşlemi
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category != null)
        {
            // GenericManager içindeki IsDeleted = true mantığı çalışacak
            await _categoryService.RemoveAsync(category);
        }
        return RedirectToAction(nameof(Index));
    }
}