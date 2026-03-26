using Blog.Core.Entities;
using Blog.Data.Repositories.Abstract;
using Blog.Data.UnitOfWorks;
using Blog.Service.Abstract;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Service.Concrete;

public class PostManager : GenericManager<Post>, IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryCache _memoryCache;

    public PostManager(IGenericRepository<Post> repository, IUnitOfWork unitOfWork, IMemoryCache memoryCache, IPostRepository postRepository)
        : base(repository, unitOfWork, memoryCache)
    {
        _postRepository = postRepository;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<Post>> GetPostsWithCategoryAsync(string? userId = null)
    {
        // Admin ve Yazar için ayrı ayrı cache key oluşturuyoruz
        string customCacheKey = $"PostsWithCategory_{userId ?? "All"}";

        if (!_memoryCache.TryGetValue(customCacheKey, out IEnumerable<Post>? cachedPosts))
        {
            cachedPosts = await _postRepository.GetPostsWithCategoryAsync(userId);
            _memoryCache.Set(customCacheKey, cachedPosts, TimeSpan.FromMinutes(10)); // 10 dakikalık cache
        }

        return cachedPosts ?? Enumerable.Empty<Post>();
    }
}