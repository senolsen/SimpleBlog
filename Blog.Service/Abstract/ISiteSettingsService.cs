using Blog.Core.Entities;

namespace Blog.Service.Abstract;

public interface ISiteSettingsService
{
    Task<SiteSetting> GetSettingsAsync();
    void InvalidateCache();
}
