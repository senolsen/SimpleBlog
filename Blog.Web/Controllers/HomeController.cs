using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly IGenericService<Page> _pageService;
    private readonly IGenericService<SiteSetting> _siteSettingService; // <-- TEKÝL OLARAK DÜZELTÝLDÝ
    private readonly IGenericService<ContactMessage> _contactMessageService;

    public HomeController(IPostService postService, IGenericService<Page> pageService, IGenericService<SiteSetting> siteSettingService, IGenericService<ContactMessage> contactMessageService)
    {
        _postService = postService;
        _pageService = pageService;
        _siteSettingService = siteSettingService;
        _contactMessageService = contactMessageService;
    }
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 6; // Her sayfada gösterilecek maksimum makale sayýsý (Ýdeal olaný 6 veya 8'dir)

        // Tüm yayýnlanmýþ ve silinmemiþ makaleleri çekiyoruz
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var publishedPosts = allPosts
            .Where(p => p.Status == PostStatus.Published && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToList();

        // Toplam makale sayýsý ve toplam sayfa sayýsýný hesaplýyoruz
        int totalPosts = publishedPosts.Count;
        int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

        // Güvenlik Önlemi: Sayfa sýnýrlarýnýn dýþýna çýkýlmasýný engelle
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        // Sihirli Bölüm: Skip ile önceki sayfalarý atla, Take ile sadece pageSize kadarýný al
        var paginatedPosts = publishedPosts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Sayfalama bilgilerini View tarafýna fýrlatýyoruz
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        string activeTheme = "ZenBlog";
        return View($"~/Views/Shared/Themes/{activeTheme}/Home/Index.cshtml", paginatedPosts);
    }

    // site.com/Hakkimizda adresine gidildiðinde çalýþacak
    [Route("Hakkimizda")]
    public async Task<IActionResult> Hakkimizda()
    {
        // DB'den slug'ý "hakkimizda" olan veriyi çekiyoruz
        var pages = await _pageService.WhereAsync(p => p.Slug == "hakkimizda" && !p.IsDeleted && p.IsActive);
        var page = pages.FirstOrDefault();

        if (page == null) return RedirectToAction("Index", "Home");

        string activeTheme = "ZenBlog";
        return View($"~/Views/Shared/Themes/{activeTheme}/Home/Hakkimizda.cshtml", page);
    }

    // ÝLETÝÞÝM SAYFASINI AÇ (GET)
    [Route("Iletisim")]
    [HttpGet]
    public async Task<IActionResult> Iletisim()
    {
        // Sistemdeki aktif ayarlarý çekiyoruz
        var settingsList = await _siteSettingService.GetAllAsync();
        var currentSettings = settingsList.FirstOrDefault();

        string activeTheme = "ZenBlog";
        return View($"~/Views/Shared/Themes/{activeTheme}/Home/Iletisim.cshtml", currentSettings);
    }

    [Route("Iletisim")]
    [HttpPost]
    public async Task<IActionResult> Iletisim(ContactMessage model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedDate = DateTime.Now;
            model.IsRead = false;

            await _contactMessageService.AddAsync(model);

            TempData["SuccessMessage"] = "Mesajýnýz baþarýyla Jetexsoft ekibine iletildi. En kýsa sürede dönüþ yapýlacaktýr.";
            return RedirectToAction("Iletisim");
        }

        TempData["ErrorMessage"] = "Lütfen formdaki zorunlu alanlarý eksiksiz doldurun.";
        return RedirectToAction("Iletisim");
    }
}