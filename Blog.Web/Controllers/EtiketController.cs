using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Data.Context; // AppDbContext için eklendi
using Blog.Service.Abstract;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync için eklendi

namespace Blog.Web.Controllers;

public class EtiketController : Controller
{
    private readonly IGenericService<Tag> _tagService;
    private readonly AppDbContext _context; // IGenericService<PostTag> yerine doğrudan DbContext kullanıyoruz
    private readonly IPostService _postService;
    private readonly IThemeService _themeService;

    public EtiketController(IGenericService<Tag> tagService, AppDbContext context, IPostService postService, IThemeService themeService)
    {
        _tagService = tagService;
        _context = context;
        _postService = postService;
        _themeService = themeService;
    }

    // Kullanım: jetexsoft.com/Etiket/csharp
    [Route("Etiket/{tagName}")]
    public async Task<IActionResult> Index(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return RedirectToAction("Index", "Home");

        var queryTag = tagName.ToLower();

        // 1. Veritabanında bu isimde bir etiket var mı buluyoruz
        var tags = await _tagService.WhereAsync(t => t.Name.ToLower() == queryTag);
        var currentTag = tags.FirstOrDefault();

        // Etiket yoksa anasayfaya at
        if (currentTag == null)
            return RedirectToAction("Index", "Home");

        // 2. ÇÖZÜM: DbContext üzerinden doğrudan çoka-çok tabloyu sorguluyoruz (Service kısıtlamasına takılmadan)
        var postIds = await _context.PostTags
            .Where(pt => pt.TagId == currentTag.Id)
            .Select(pt => pt.PostId)
            .ToListAsync();

        // 3. Makaleleri çekip filtreliyoruz
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var results = allPosts
            .Where(p => postIds.Contains(p.Id) && p.Status == PostStatus.Published && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToList();

        // Ekranda orijinal büyük/küçük harfiyle göstermek için ViewBag'e atıyoruz
        ViewBag.TagName = currentTag.Name;

        return View(_themeService.GetViewPath("Etiket/Index"), results);
    }
}