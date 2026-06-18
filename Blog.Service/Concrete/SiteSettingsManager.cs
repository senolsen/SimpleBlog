using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Service.Concrete;

public class SiteSettingsManager : ISiteSettingsService
{
    private const string CacheKey = "SiteSettings_Singleton";

    private readonly AppDbContext _context;
    private readonly IMemoryCache _memoryCache;

    public SiteSettingsManager(AppDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    public async Task<SiteSetting> GetSettingsAsync()
    {
        if (_memoryCache.TryGetValue(CacheKey, out SiteSetting? cached) && cached != null)
            return cached;

        var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
        settings ??= new SiteSetting
        {
            SiteTitle = "SimpleBlog",
            ActiveTheme = "MagDesign"
        };

        _memoryCache.Set(CacheKey, settings, new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(15))
            .SetAbsoluteExpiration(TimeSpan.FromHours(2)));

        return settings;
    }

    public void InvalidateCache() => _memoryCache.Remove(CacheKey);
}
