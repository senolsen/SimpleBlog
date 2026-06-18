using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly IGenericService<Page> _pageService;
    private readonly IGenericService<ContactMessage> _messageService;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IThemeService _themeService;

    public HomeController(
        IPostService postService,
        IGenericService<Page> pageService,
        IGenericService<ContactMessage> messageService,
        ISiteSettingsService siteSettingsService,
        IThemeService themeService)
    {
        _postService = postService;
        _pageService = pageService;
        _messageService = messageService;
        _siteSettingsService = siteSettingsService;
        _themeService = themeService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 6;
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var published = allPosts
            .Where(p => p.Status == PostStatus.Published && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToList();

        if (page < 1) page = 1;
        var totalPages = Math.Max(1, (int)Math.Ceiling(published.Count / (double)pageSize));
        if (page > totalPages) page = totalPages;

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        var pagePosts = published.Skip((page - 1) * pageSize).Take(pageSize);
        return View(_themeService.GetViewPath("Home/Index"), pagePosts);
    }

    [Route("Hakkimizda")]
    public async Task<IActionResult> Hakkimizda()
    {
        var pages = await _pageService.WhereAsync(p => p.Slug == "hakkimizda" && !p.IsDeleted && p.IsActive);
        var page = pages.FirstOrDefault() ?? new Page
        {
            Title = "Hakk?m?zda",
            Content = "<p>Bu sayfan?n i�eri?ini admin panelinden d�zenleyebilirsiniz. Slug de?eri <strong>hakkimizda</strong> olan bir sayfa olu?turun.</p>"
        };

        return View(_themeService.GetViewPath("Home/Hakkimizda"), page);
    }

    [Route("Iletisim")]
    [HttpGet]
    public async Task<IActionResult> Iletisim()
    {
        var settings = await _siteSettingsService.GetSettingsAsync();
        return View(_themeService.GetViewPath("Home/Iletisim"), settings);
    }

    [Route("Iletisim")]
    [HttpPost]
    public async Task<IActionResult> Iletisim(string name, string email, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            TempData["ErrorMessage"] = "L�tfen t�m alanlar? eksiksiz doldurun.";
            return RedirectToAction(nameof(Iletisim));
        }

        await _messageService.AddAsync(new ContactMessage
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Subject = subject.Trim(),
            Message = message.Trim(),
            IsRead = false
        });

        TempData["SuccessMessage"] = "Mesaj?n?z ba?ar?yla g�nderildi. En k?sa s�rede size d�n�? yapaca??z.";
        return RedirectToAction(nameof(Iletisim));
    }
}
