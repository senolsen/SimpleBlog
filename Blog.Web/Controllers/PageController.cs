using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class PageController : Controller
{
    private readonly IGenericService<Page> _pageService;

    public PageController(IGenericService<Page> pageService)
    {
        _pageService = pageService;
    }

    public async Task<IActionResult> Detail(string slug)
    {
        // Slug boşsa doğrudan 404'e at
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        // WhereAsync ile filtreleyip dönen listeden ilk kaydı (FirstOrDefault) alıyoruz
        var pages = await _pageService.WhereAsync(x => x.Slug == slug && x.IsActive && !x.IsDeleted);
        var page = pages.FirstOrDefault();

        // Eğer veritabanında da böyle bir sayfa yoksa, gerçekten 404 (Bulunamadı) sayfası ver
        if (page == null)
        {
            return NotFound();
        }

        // Sayfa bulunduysa View'a gönder
        return View(page);
    }
}