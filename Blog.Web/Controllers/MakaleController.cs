using Blog.Core.Enums;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class MakaleController : Controller
{
    private readonly IPostService _postService;

    public MakaleController(IPostService postService)
    {
        _postService = postService;
    }

    // Arama motoru dostu (SEO) URL yapımız: jetexsoft.com/Makale/makale-basligi
    [Route("Makale/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return RedirectToAction("Index", "Home");

        // Makaleyi kategorisiyle birlikte çekiyoruz
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var post = allPosts.FirstOrDefault(p => p.Slug == slug && p.Status == PostStatus.Published && !p.IsDeleted);

        // Eğer makale bulunamazsa veya silinmişse Ana Sayfaya (veya 404'e) yönlendir
        if (post == null)
            return RedirectToAction("Index", "Home");

        // Okunma sayısını 1 artır ve veritabanına kaydet
        post.ViewCount += 1;
        await _postService.UpdateAsync(post);

        // Çoklu Tema Mimarisi Yönlendirmesi
        string activeTheme = "ZenBlog";
        string viewPath = $"~/Views/Shared/Themes/{activeTheme}/Makale/Details.cshtml";

        return View(viewPath, post);
    }
}