using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.ViewComponents;

public class SidebarPopularPostsViewComponent : ViewComponent
{
    private readonly IPostService _postService;

    public SidebarPopularPostsViewComponent(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Tüm makaleleri çekiyoruz
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);

        // Yayında olanları filtreleyip, Okunma Sayısına (ViewCount) göre azalan şekilde sıralıyor
        // ve ekranda kalabalık yapmaması için sadece en popüler 4 makaleyi alıyoruz (Take(4)).
        var popularPosts = allPosts
            .Where(p => p.Status == PostStatus.Published && !p.IsDeleted)
            .OrderByDescending(p => p.ViewCount)
            .Take(4)
            .ToList();

        return View(popularPosts);
    }
}