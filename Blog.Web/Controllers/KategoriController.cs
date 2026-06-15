using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class KategoriController : Controller
{
    private readonly IPostService _postService;
    private readonly IGenericService<Category> _categoryService;

    public KategoriController(IPostService postService, IGenericService<Category> categoryService)
    {
        _postService = postService;
        _categoryService = categoryService;
    }

    // Yönlendirme yapımız: site.com/Kategori/kategori-slug
    [Route("Kategori/{slug}")]
    public async Task<IActionResult> Index(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return RedirectToAction("Index", "Home");

        // Önce URL'den gelen slug ile kategoriyi buluyoruz
        var categoryList = await _categoryService.WhereAsync(c => c.Slug == slug && !c.IsDeleted && c.IsActive);
        var category = categoryList.FirstOrDefault();

        // Kategori yoksa ana sayfaya dön
        if (category == null)
            return RedirectToAction("Index", "Home");

        // Tüm makaleleri çek ve bu kategoriye ait olanları filtrele
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var categoryPosts = allPosts
            .Where(p => p.CategoryId == category.Id && p.Status == PostStatus.Published && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToList();

        // Kategori adını tasarımda (View) kullanmak için ViewBag ile gönderiyoruz
        ViewBag.CategoryName = category.Name;

        // Çoklu Tema Mimarisi
        string activeTheme = "ZenBlog";
        string viewPath = $"~/Views/Shared/Themes/{activeTheme}/Kategori/Index.cshtml";

        return View(viewPath, categoryPosts);
    }
}