using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Data.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories.Concrete;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Post>> GetPostsWithCategoryAsync(string? userId = null)
    {
        // BURASI KRİTİK: .Where(x => !x.IsDeleted) ekleyerek silinmişleri es geçiyoruz
        var query = _context.Posts
                            .Include(x => x.Category)
                            .Include(x => x.AppUser)
                            .Where(x => !x.IsDeleted);

        // Eğer Yazar rolüyse sadece kendi yazılarını görsün
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(x => x.AppUserId == userId);
        }

        // Tarihe göre yeniden eskiye (en son eklenen en üstte) sıralayarak gönder
        return await query.OrderByDescending(x => x.CreatedDate).ToListAsync();
    }
    public async Task<Post?> GetPostByIdWithTagsAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.PostTags) // Makalenin etiket bağlarını da getir
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

}