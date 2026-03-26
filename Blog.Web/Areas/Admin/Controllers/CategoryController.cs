using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Blog.Web.Helpers; // Slug üretici için eklendi
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör")] // Yazar rolü kategori ekleyip silemez
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
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var category = new Category
            {
                Name = model.Name,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                Slug = UrlHelper.GenerateSlug(model.Name) // Otomatik Slug
            };

            await _categoryService.AddAsync(category);
            TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null || category.IsDeleted) return NotFound();

        var model = new CategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var category = await _categoryService.GetByIdAsync(model.Id);
            if (category == null || category.IsDeleted) return NotFound();

            category.Name = model.Name;
            category.MetaTitle = model.MetaTitle;
            category.MetaDescription = model.MetaDescription;
            category.Slug = UrlHelper.GenerateSlug(model.Name); // Güncellenen Slug

            await _categoryService.UpdateAsync(category);
            TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category != null)
        {
            await _categoryService.RemoveAsync(category);
            TempData["SuccessMessage"] = "Kategori başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}