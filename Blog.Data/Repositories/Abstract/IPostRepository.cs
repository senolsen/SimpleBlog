using Blog.Core.Entities;

namespace Blog.Data.Repositories.Abstract;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<IEnumerable<Post>> GetPostsWithCategoryAsync(string? userId = null);
    Task<Post?> GetPostByIdWithTagsAsync(int id);
}