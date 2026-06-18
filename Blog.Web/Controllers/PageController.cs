using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class PageController : Controller
{
    private readonly IGenericService<Page> _pageService;
    private readonly IThemeService _themeService;

    public PageController(IGenericService<Page> pageService, IThemeService themeService)
    {
        _pageService = pageService;
        _themeService = themeService;
    }

    public async Task<IActionResult> Detail(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var pages = await _pageService.WhereAsync(x => x.Slug == slug && x.IsActive && !x.IsDeleted);
        var page = pages.FirstOrDefault();

        if (page == null)
            return NotFound();

        return View(_themeService.GetViewPath("Page/Detail"), page);
    }
}