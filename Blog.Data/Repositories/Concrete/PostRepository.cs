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
        var query = _context.Posts
            .Include(p => p.Category)
            .Include(p => p.AppUser)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(p => p.AppUserId == userId);

        return await query.OrderByDescending(p => p.CreatedDate).ToListAsync();
    }
}