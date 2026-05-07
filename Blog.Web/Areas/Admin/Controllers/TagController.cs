using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Blog.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör")] // Yazar etiket ekleyemez
public class TagController : Controller
{
    private readonly IGenericService<Tag> _tagService;

    public TagController(IGenericService<Tag> tagService)
    {
        _tagService = tagService;
    }

    public async Task<IActionResult> Index()
    {
        var tags = await _tagService.WhereAsync(t => !t.IsDeleted);
        return View(tags);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TagCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var tag = new Tag
            {
                Name = model.Name,
                Slug = UrlHelper.GenerateSlug(model.Name) // Slug otomatik üretilir
            };
            await _tagService.AddAsync(tag);
            TempData["SuccessMessage"] = "Etiket eklendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag == null || tag.IsDeleted) return NotFound();

        var model = new TagEditViewModel { Id = tag.Id, Name = tag.Name };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TagEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var tag = await _tagService.GetByIdAsync(model.Id);
            if (tag == null) return NotFound();

            tag.Name = model.Name;
            tag.Slug = UrlHelper.GenerateSlug(model.Name);

            await _tagService.UpdateAsync(tag);
            TempData["SuccessMessage"] = "Etiket güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag != null)
        {
            await _tagService.RemoveAsync(tag);
            TempData["SuccessMessage"] = "Etiket silindi.";
        }
        return RedirectToAction(nameof(Index));
    }
}