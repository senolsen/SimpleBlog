using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.ViewComponents;

public class SidebarCategoriesViewComponent : ViewComponent
{
    private readonly IGenericService<Category> _categoryService;
    private readonly IPostService _postService; // Makaleleri saymak için ekledik

    public SidebarCategoriesViewComponent(IGenericService<Category> categoryService, IPostService postService)
    {
        _categoryService = categoryService;
        _postService = postService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Silinmemiş ve aktif kategorileri çek
        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted && c.IsActive);

        // 2. Makaleleri çek
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);

        // 3. Kategori ve o kategoriye ait yayınlanmış makale sayısını eşleştir
        var categoryDictionary = new Dictionary<Category, int>();

        foreach (var category in categories)
        {
            // Sadece bu kategoriye ait, silinmemiş ve YAYINDA olan makaleleri say
            int postCount = allPosts.Count(p => p.CategoryId == category.Id && p.Status == PostStatus.Published && !p.IsDeleted);

            categoryDictionary.Add(category, postCount);
        }

        // 4. İstersen hiç makalesi olmayanları gizlemek için alttaki satırı kullanabilirsin ama şimdilik hepsini gönderiyoruz
         categoryDictionary = categoryDictionary.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);

        return View(categoryDictionary);
    }
}