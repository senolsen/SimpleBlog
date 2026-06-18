using Blog.Core.Entities;
using Blog.Service.Abstract;

namespace Blog.Web.Services;

public class ThemeService : IThemeService
{
    public const string DefaultTheme = "MagDesign";
    public const string HttpContextKey = "ActiveTheme";

    private readonly IWebHostEnvironment _env;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ThemeService(
        IWebHostEnvironment env,
        ISiteSettingsService siteSettingsService,
        IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _siteSettingsService = siteSettingsService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> ResolveActiveThemeAsync(SiteSetting? settings = null)
    {
        settings ??= await _siteSettingsService.GetSettingsAsync();
        var candidate = string.IsNullOrWhiteSpace(settings.ActiveTheme) ? DefaultTheme : settings.ActiveTheme.Trim();
        return IsThemeValid(candidate) ? candidate : DefaultTheme;
    }

    public string GetViewPath(string relativeView)
    {
        var theme = _httpContextAccessor.HttpContext?.Items[HttpContextKey]?.ToString() ?? DefaultTheme;
        if (!IsThemeValid(theme))
            theme = DefaultTheme;

        return $"~/Views/Shared/Themes/{theme}/{relativeView}.cshtml";
    }

    public IReadOnlyList<string> GetAvailableThemes()
    {
        var themesRoot = Path.Combine(_env.ContentRootPath, "Views", "Shared", "Themes");
        if (!Directory.Exists(themesRoot))
            return [DefaultTheme];

        return Directory.GetDirectories(themesRoot)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name) && IsThemeValid(name!))
            .OrderBy(name => name)
            .Cast<string>()
            .ToList();
    }

    private bool IsThemeValid(string themeName)
    {
        var layoutPath = Path.Combine(_env.ContentRootPath, "Views", "Shared", "Themes", themeName, "_Layout.cshtml");
        if (!System.IO.File.Exists(layoutPath))
            return false;

        var content = System.IO.File.ReadAllText(layoutPath);
        return content.Contains("@RenderBody()", StringComparison.OrdinalIgnoreCase);
    }
}
