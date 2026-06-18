using Blog.Core.Entities;

namespace Blog.Web.Services;

public interface IThemeService
{
    Task<string> ResolveActiveThemeAsync(SiteSetting? settings = null);
    string GetViewPath(string relativeView);
    IReadOnlyList<string> GetAvailableThemes();
}
