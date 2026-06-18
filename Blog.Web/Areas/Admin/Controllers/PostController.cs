using System.Security.Claims;
using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Blog.Web.Helpers;
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
    private readonly IGenericService<Tag> _tagService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PostController(IPostService postService, IGenericService<Category> categoryService, IGenericService<Tag> tagService, IWebHostEnvironment webHostEnvironment)
    {
        _postService = postService;
        _categoryService = categoryService;
        _tagService = tagService;
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

        var tags = await _tagService.WhereAsync(t => !t.IsDeleted);
        ViewBag.Tags = new SelectList(tags, "Id", "Name");

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
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                Slug = UrlHelper.GenerateSlug(model.Title),
                Status = model.Status
            };

            post.PostTags = new List<PostTag>();
            if (model.SelectedTags != null && model.SelectedTags.Any())
            {
                var mevcutEtiketler = await _tagService.WhereAsync(t => !t.IsDeleted);

                foreach (var item in model.SelectedTags)
                {
                    if (int.TryParse(item, out int tagId) && mevcutEtiketler.Any(t => t.Id == tagId))
                    {
                        post.PostTags.Add(new PostTag { TagId = tagId });
                    }
                    else
                    {
                        var newTag = new Tag
                        {
                            Name = item,
                            Slug = UrlHelper.GenerateSlug(item),
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };
                        await _tagService.AddAsync(newTag);
                        post.PostTags.Add(new PostTag { TagId = newTag.Id });
                    }
                }
            }

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

                post.CoverImagePath = await ImageHelper.UploadAndConvertToWebpAsync(model.CoverImageFile, _webHostEnvironment.WebRootPath, "posts");
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
        var post = await _postService.GetPostByIdWithTagsAsync(id);
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
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            SelectedTags = post.PostTags?.Select(pt => pt.TagId.ToString()).ToList() ?? new List<string>(),
            Status = post.Status
        };

        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", post.CategoryId);

        var tags = await _tagService.WhereAsync(t => !t.IsDeleted);
        ViewBag.Tags = new SelectList(tags, "Id", "Name");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PostEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var post = await _postService.GetPostByIdWithTagsAsync(model.Id);
            if (post == null) return NotFound();

            if (User.IsInRole("Yazar") && post.AppUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "Admin" });
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.CategoryId = model.CategoryId;
            post.MetaTitle = model.MetaTitle;
            post.MetaDescription = model.MetaDescription;
            post.Slug = UrlHelper.GenerateSlug(model.Title);

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

                if (!string.IsNullOrEmpty(post.CoverImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                post.CoverImagePath = await ImageHelper.UploadAndConvertToWebpAsync(model.NewCoverImageFile, _webHostEnvironment.WebRootPath, "posts");
            }

            post.PostTags ??= new List<PostTag>();
            post.PostTags.Clear();

            if (model.SelectedTags != null && model.SelectedTags.Any())
            {
                var mevcutEtiketler = await _tagService.WhereAsync(t => !t.IsDeleted);

                foreach (var item in model.SelectedTags)
                {
                    if (int.TryParse(item, out int tagId) && mevcutEtiketler.Any(t => t.Id == tagId))
                    {
                        post.PostTags.Add(new PostTag { TagId = tagId, PostId = post.Id });
                    }
                    else
                    {
                        var newTag = new Tag
                        {
                            Name = item,
                            Slug = UrlHelper.GenerateSlug(item),
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };
                        await _tagService.AddAsync(newTag);
                        post.PostTags.Add(new PostTag { TagId = newTag.Id, PostId = post.Id });
                    }
                }
            }

            post.Status = model.Status;
            await _postService.UpdateAsync(post);
            TempData["SuccessMessage"] = "Yazı başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post != null)
        {
            if (User.IsInRole("Yazar") && post.AppUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            await _postService.RemoveAsync(post);
            return Ok();
        }
        return NotFound();
    }

    // YENİ: EDİTÖRDEN GELEN RESİMLERİ KARŞILAYIP WEBP YAPAN METOT
    [HttpPost]
    public async Task<IActionResult> UploadEditorImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Dosya seçilmedi." });
        }

        const int maxFileSize = 5 * 1024 * 1024; // 5MB Sınırı
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (file.Length > maxFileSize || !allowedExtensions.Contains(extension))
        {
            return Json(new { success = false, message = "Geçersiz dosya boyutu veya uzantısı." });
        }

        // posts klasörünün içine "editor" adında alt klasöre kaydediyoruz ki ana kapak resimleriyle karışmasın
        var imagePath = await ImageHelper.UploadAndConvertToWebpAsync(file, _webHostEnvironment.WebRootPath, "posts/editor");

        return Json(new { success = true, url = imagePath });
    }
}