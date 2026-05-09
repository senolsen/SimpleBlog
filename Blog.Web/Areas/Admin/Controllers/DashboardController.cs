using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör,Yazar")]
public class DashboardController : Controller
{
    private readonly IPostService _postService;
    private readonly IGenericService<Category> _categoryService;
    private readonly IGenericService<Comment> _commentService;

    public DashboardController(IPostService postService, IGenericService<Category> categoryService, IGenericService<Comment> commentService)
    {
        _postService = postService;
        _categoryService = categoryService;
        _commentService = commentService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdminOrEditor = User.IsInRole("Admin") || User.IsInRole("Editör");

        // 1. Verileri Rollere Göre Çek
        var postsQuery = await _postService.WhereAsync(p => !p.IsDeleted);
        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        var commentsQuery = await _commentService.WhereAsync(c => !c.IsDeleted);

        // Eğer kullanıcı sadece Yazarsa, verileri filtrele
        if (!isAdminOrEditor)
        {
            postsQuery = postsQuery.Where(p => p.AppUserId == currentUserId).ToList();

            // Sadece yazarın kendi yazılarına gelen yorumları bul
            var userPostIds = postsQuery.Select(p => p.Id).ToList();
            commentsQuery = commentsQuery.Where(c => userPostIds.Contains(c.PostId)).ToList();
        }

        // 2. Kategori Dağılımını Hesapla (En Popüler İlk 10 Kategori)
        var topCategoryStats = categories
            .Select(cat => new
            {
                CategoryName = cat.Name,
                PostCount = postsQuery.Count(p => p.CategoryId == cat.Id)
            })
            .Where(x => x.PostCount > 0) // Hiç yazısı olmayanları ele
            .OrderByDescending(x => x.PostCount) // Yazı sayısına göre büyükten küçüğe sırala
            .Take(10) // Sadece ilk 10'u al!
            .ToList();

        // Senin ViewModel'in beklediği formatta listeleri oluşturuyoruz
        var categoryNames = topCategoryStats.Select(x => x.CategoryName).ToList();
        var postCounts = topCategoryStats.Select(x => x.PostCount).ToList();

        // Ön taraftaki (View) tablo için veriyi ViewBag içine atıyoruz
        ViewBag.TopCategories = topCategoryStats;

        // 3. Modeli Doldur
        var model = new DashboardViewModel
        {
            TotalPosts = postsQuery.Count(),
            TotalCategories = categories.Count(),
            TotalComments = commentsQuery.Count(),
            TotalViews = postsQuery.Sum(p => p.ViewCount),

            TopPosts = postsQuery.OrderByDescending(p => p.ViewCount).Take(10).ToList(),

            ChartCategoryNames = categoryNames,
            ChartPostCounts = postCounts
        };

        // 4. TÜM KATEGORİLERİN LİSTESİ (Tablo İçin)
        var allCategoryStats = categories
            .Select(cat => new
            {
                CategoryName = cat.Name,
                PostCount = postsQuery.Count(p => p.CategoryId == cat.Id)
            })
            .OrderByDescending(x => x.PostCount) // En çok yazısı olandan başlayarak sırala
            .ToList();

        ViewBag.AllCategoryStats = allCategoryStats;

        return View(model);
    }
}