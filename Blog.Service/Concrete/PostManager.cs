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

    public async Task<Post?> GetPostByIdWithTagsAsync(int id)
    {
        return await _postRepository.GetPostByIdWithTagsAsync(id);
    }

    public async Task IncreaseViewCountAsync(int id)
    {
        var post = await _repository.GetByIdAsync(id);
        if (post != null)
        {
            post.ViewCount += 1; // Sayacı 1 artır

            _repository.Update(post);
            await _unitOfWork.CommitAsync();

            // Not: Sadece sayacı artırıp, Global Cache'e dokunmuyoruz.
        }
    }

    public virtual async Task AddAsync(Post entity)
    {
        await base.AddAsync(entity); // Asıl kayıt işlemini yap
        ClearCustomCache(entity.AppUserId); // Sonra özel önbelleğimizi çöpe at
    }

    public virtual async Task UpdateAsync(Post entity)
    {
        await base.UpdateAsync(entity);
        ClearCustomCache(entity.AppUserId);
    }

    public virtual async Task RemoveAsync(Post entity)
    {
        await base.RemoveAsync(entity);
        ClearCustomCache(entity.AppUserId);
    }

    // Sadece bu sınıfa özel ürettiğimiz önbellek anahtarlarını silen yardımcı metot
    private void ClearCustomCache(string? userId)
    {
        _memoryCache.Remove("PostsWithCategory_All");

        if (!string.IsNullOrEmpty(userId))
        {
            _memoryCache.Remove($"PostsWithCategory_{userId}");
        }
    }
}