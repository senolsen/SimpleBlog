using System.Security.Claims;
using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Blog.Web.Helpers; // SLUG ÜRETİCİ İÇİN EKLENDİ
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör,Yazar")]
public class PostController : Controller
{
    private readonly IPostService _postService;
    private readonly IGenericService<Category> _categoryService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PostController(IPostService postService, IGenericService<Category> categoryService, IWebHostEnvironment webHostEnvironment)
    {
        _postService = postService;
        _categoryService = categoryService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? filterUserId = User.IsInRole("Yazar") ? currentUserId : null;
        var posts = await _postService.GetPostsWithCategoryAsync(filterUserId);
        return View(posts);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                CategoryId = model.CategoryId,
                AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),

                // YENİ SEO ALANLARI VE OTOMATİK SLUG
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                Slug = UrlHelper.GenerateSlug(model.Title)
            };

            // ... (Resim yükleme kodları aynı kalıyor) ...
            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                const int maxFileSize = 5 * 1024 * 1024;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.CoverImageFile.FileName).ToLowerInvariant();

                if (model.CoverImageFile.Length > maxFileSize || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("CoverImageFile", "Geçersiz dosya boyutu veya uzantısı.");
                    var catsForError = await _categoryService.WhereAsync(c => !c.IsDeleted);
                    ViewBag.Categories = new SelectList(catsForError, "Id", "Name");
                    return View(model);
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImageFile.CopyToAsync(fileStream);
                }

                post.CoverImagePath = "/uploads/" + uniqueFileName;
            }

            await _postService.AddAsync(post);
            TempData["SuccessMessage"] = "Yazı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post == null || post.IsDeleted) return NotFound();

        if (User.IsInRole("Yazar") && post.AppUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return RedirectToAction("AccessDenied", "Auth", new { area = "Admin" });
        }

        var model = new PostEditViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CategoryId = post.CategoryId,
            ExistingImagePath = post.CoverImagePath,

            // YENİ SEO ALANLARINI MODEL'E YÜKLÜYORUZ
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription
        };

        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", post.CategoryId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PostEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var post = await _postService.GetByIdAsync(model.Id);
            if (post == null) return NotFound();

            if (User.IsInRole("Yazar") && post.AppUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "Admin" });
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.CategoryId = model.CategoryId;

            // YENİ SEO ALANLARI VE GÜNCELLENEN SLUG
            post.MetaTitle = model.MetaTitle;
            post.MetaDescription = model.MetaDescription;
            post.Slug = UrlHelper.GenerateSlug(model.Title);

            // ... (Resim yükleme ve silme kodları aynı kalıyor) ...
            if (model.NewCoverImageFile != null && model.NewCoverImageFile.Length > 0)
            {
                const int maxFileSize = 5 * 1024 * 1024;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.NewCoverImageFile.FileName).ToLowerInvariant();

                if (model.NewCoverImageFile.Length > maxFileSize || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("NewCoverImageFile", "Geçersiz dosya boyutu veya uzantısı.");
                    var cats = await _categoryService.WhereAsync(c => !c.IsDeleted);
                    ViewBag.Categories = new SelectList(cats, "Id", "Name", model.CategoryId);
                    return View(model);
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewCoverImageFile.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(post.CoverImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                post.CoverImagePath = "/uploads/" + uniqueFileName;
            }

            await _postService.UpdateAsync(post);
            TempData["SuccessMessage"] = "Yazı başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post != null)
        {
            if (User.IsInRole("Yazar") && post.AppUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "Admin" });
            }

            await _postService.RemoveAsync(post);
            TempData["SuccessMessage"] = "Yazı başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}