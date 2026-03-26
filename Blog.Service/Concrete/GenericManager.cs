using System.Linq.Expressions;
using Blog.Core.Entities;
using Blog.Data.Repositories.Abstract;
using Blog.Data.UnitOfWorks;
using Blog.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Service.Concrete;

public class GenericManager<T> : IGenericService<T> where T : BaseEntity
{
    private readonly IGenericRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _memoryCache;
    private readonly string _cacheKey;

    public GenericManager(IGenericRepository<T> repository, IUnitOfWork unitOfWork, IMemoryCache memoryCache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _memoryCache = memoryCache;
        _cacheKey = $"{typeof(T).Name}_GetAll"; // Dinamik cache anahtarı (Örn: Post_GetAll, Category_GetAll)
    }

    public async Task<T?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        // Önce Cache'e bak, yoksa veritabanından çek ve Cache'e yaz
        if (!_memoryCache.TryGetValue(_cacheKey, out IEnumerable<T>? cachedList))
        {
            cachedList = await _repository.GetAll().ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(15)) // 15 dk boyunca kimse girmezse sil
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));  // Her halükarda 2 saat sonra sil (veri tazele)

            _memoryCache.Set(_cacheKey, cachedList, cacheOptions);
        }

        return cachedList ?? Enumerable.Empty<T>();
    }

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        => await _repository.Where(predicate).ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _repository.AddAsync(entity);
        await _unitOfWork.CommitAsync();

        _memoryCache.Remove(_cacheKey); // Yeni veri eklendi, eski cache'i patlat
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        entity.UpdatedDate = DateTime.Now;
        _repository.Update(entity);
        await _unitOfWork.CommitAsync();

        _memoryCache.Remove(_cacheKey); // Veri güncellendi, eski cache'i patlat
    }

    public async Task RemoveAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.Now;

        _repository.Update(entity);
        await _unitOfWork.CommitAsync();

        _memoryCache.Remove(_cacheKey); // Veri silindi, eski cache'i patlat
    }
}