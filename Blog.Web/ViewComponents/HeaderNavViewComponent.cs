using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Blog.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.ViewComponents;

public class HeaderNavViewComponent : ViewComponent
{
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IGenericService<Category> _categoryService;
    private readonly IPostService _postService;

    public HeaderNavViewComponent(
        ISiteSettingsService siteSettingsService,
        IGenericService<Category> categoryService,
        IPostService postService)
    {
        _siteSettingsService = siteSettingsService;
        _categoryService = categoryService;
        _postService = postService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string part = "header")
    {
        var model = await BuildModelAsync();
        return part == "drawer" ? View("Drawer", model) : View("Default", model);
    }

    private async Task<HeaderNavViewModel> BuildModelAsync()
    {
        var settings = await _siteSettingsService.GetSettingsAsync();
        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted && c.IsActive);
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);

        var categoryCounts = categories
            .Select(c => new HeaderCategoryItem
            {
                Name = c.Name,
                Slug = c.Slug ?? string.Empty,
                PostCount = allPosts.Count(p => p.CategoryId == c.Id && p.Status == PostStatus.Published && !p.IsDeleted)
            })
            .Where(c => c.PostCount > 0)
            .OrderByDescending(c => c.PostCount)
            .Take(12)
            .ToList();

        return new HeaderNavViewModel
        {
            Settings = settings,
            Categories = categoryCounts,
            CurrentPath = ViewContext.HttpContext.Request.Path.Value ?? "/"
        };
    }
}
