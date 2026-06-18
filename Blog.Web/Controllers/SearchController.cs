using Blog.Core.Enums;
using Blog.Service.Abstract;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class SearchController : Controller
{
    private readonly IPostService _postService;
    private readonly IThemeService _themeService;

    public SearchController(IPostService postService, IThemeService themeService)
    {
        _postService = postService;
        _themeService = themeService;
    }

    // site.com/Search?q=yazilim
    public async Task<IActionResult> Index(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return RedirectToAction("Index", "Home");

        // Tüm yayınlanmış makaleleri çekiyoruz
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);

        // Kelimeyi küçük harfe çevirip; başlıkta, içerikte veya kategoride arıyoruz
        var query = q.ToLower();
        var results = allPosts
            .Where(p => p.Status == PostStatus.Published && !p.IsDeleted &&
                        (p.Title.ToLower().Contains(query) ||
                         p.Content.ToLower().Contains(query) ||
                         p.Category.Name.ToLower().Contains(query)))
            .OrderByDescending(p => p.CreatedDate)
            .ToList();

        ViewBag.SearchQuery = q; // Aranan kelimeyi ekranda göstermek için

        return View(_themeService.GetViewPath("Search/Index"), results);
    }
}