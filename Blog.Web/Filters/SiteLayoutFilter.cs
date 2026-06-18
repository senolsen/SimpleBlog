using Blog.Service.Abstract;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Web.Filters;

public class SiteLayoutFilter : IAsyncActionFilter
{
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IThemeService _themeService;

    public SiteLayoutFilter(ISiteSettingsService siteSettingsService, IThemeService themeService)
    {
        _siteSettingsService = siteSettingsService;
        _themeService = themeService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values.TryGetValue("area", out var area) && area?.ToString() == "Admin")
        {
            await next();
            return;
        }

        var settings = await _siteSettingsService.GetSettingsAsync();
        var activeTheme = await _themeService.ResolveActiveThemeAsync(settings);
        var assetsPath = $"/themes/{activeTheme.ToLowerInvariant()}/assets";

        context.HttpContext.Items[ThemeService.HttpContextKey] = activeTheme;

        if (context.Controller is Controller controller)
        {
            controller.ViewData["ActiveTheme"] = activeTheme;
            controller.ViewData["ThemeAssetsPath"] = assetsPath;
            controller.ViewData["SiteSettings"] = settings;
            controller.ViewData["CurrentPath"] = context.HttpContext.Request.Path.Value ?? "/";
        }

        await next();
    }
}
