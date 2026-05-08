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

        // 2. Kategori Dağılımını Hesapla
        var categoryNames = new List<string>();
        var postCounts = new List<int>();

        foreach (var cat in categories)
        {
            var count = postsQuery.Count(p => p.CategoryId == cat.Id);
            if (count > 0) // Sadece içinde yazı olan kategorileri grafikte göster
            {
                categoryNames.Add(cat.Name);
                postCounts.Add(count);
            }
        }

        // 3. Modeli Doldur
        var model = new DashboardViewModel
        {
            TotalPosts = postsQuery.Count(),
            TotalCategories = categories.Count(), // Bu bilgi genel kalabilir veya yazarın kullandığı kategoriler yapılabilir
            TotalComments = commentsQuery.Count(),
            TotalViews = postsQuery.Sum(p => p.ViewCount),

            TopPosts = postsQuery.OrderByDescending(p => p.ViewCount).Take(10).ToList(),

            ChartCategoryNames = categoryNames,
            ChartPostCounts = postCounts
        };

        return View(model);
    }
}