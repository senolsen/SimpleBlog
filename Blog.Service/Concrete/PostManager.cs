using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Blog.Service.Concrete;

public class PostManager : GenericManager<Post>, IPostService
{
    public PostManager(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Post>> GetPostsWithCategoryAsync(string? userId = null)
    {
        var query = _context.Posts
            .Include(p => p.Category)
            .Include(p => p.AppUser) // Tabloya yazar bilgisini de dahil ediyoruz
            .Where(p => !p.IsDeleted);

        // Eğer dışarıdan bir userId gönderilmişse (Yazar giriş yapmışsa), sadece onun yazılarını filtrele
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(p => p.AppUserId == userId);
        }

        return await query.OrderByDescending(p => p.CreatedDate).ToListAsync();
    }
}