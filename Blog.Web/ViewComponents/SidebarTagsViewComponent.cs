using Blog.Core.Enums;
using Blog.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.ViewComponents;

public class SidebarTagsViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public SidebarTagsViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Limiti 100'e çıkararak arama kutusunun geniş bir havuzda çalışmasını sağlıyoruz
        var popularTags = await _context.Tags
            .Where(t => t.PostTags.Any(pt => pt.Post.Status == PostStatus.Published && !pt.Post.IsDeleted))
            .OrderByDescending(t => t.PostTags.Count(pt => pt.Post.Status == PostStatus.Published && !pt.Post.IsDeleted))
            .Take(100)
            .ToListAsync();

        return View(popularTags);
    }
}