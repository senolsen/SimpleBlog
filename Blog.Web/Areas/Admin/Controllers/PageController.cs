using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Blog.Core.Helpers; // SlugHelper için

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör")]
public class PageController : Controller
{
    private readonly IGenericService<Page> _pageService;

    public PageController(IGenericService<Page> pageService)
    {
        _pageService = pageService;
    }

    public async Task<IActionResult> Index()
    {
        var pages = await _pageService.WhereAsync(x => !x.IsDeleted);
        return View(pages);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Page page)
    {
        page.Slug = SlugHelper.MakeSlug(page.Title); // Başlıktan otomatik URL üret
        await _pageService.AddAsync(page);
        TempData["SuccessMessage"] = "Sayfa başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var page = await _pageService.GetByIdAsync(id);
        return View(page);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Page page)
    {
        page.Slug = SlugHelper.MakeSlug(page.Title);
        await _pageService.UpdateAsync(page);
        TempData["SuccessMessage"] = "Sayfa güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var page = await _pageService.GetByIdAsync(id);
        if (page != null) await _pageService.RemoveAsync(page);
        return RedirectToAction(nameof(Index));
    }
}