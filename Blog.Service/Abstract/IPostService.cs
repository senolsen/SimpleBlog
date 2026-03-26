using Blog.Core.Entities;

namespace Blog.Service.Abstract;

public interface IPostService : IGenericService<Post>
{
    Task<IEnumerable<Post>> GetPostsWithCategoryAsync(string? userId = null);
}