using Blog.Service.Abstract;
using Blog.Web.Helpers;

namespace Blog.Web.Middlewares;

public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISiteSettingsService siteSettingsService)
    {
        var path = context.Request.Path;

        if (IsExemptPath(path))
        {
            await _next(context);
            return;
        }

        var settings = await siteSettingsService.GetSettingsAsync();
        if (!settings.IsMaintenanceMode)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers["Retry-After"] = "3600";
        context.Response.ContentType = "text/html; charset=utf-8";

        var html = MaintenancePageBuilder.Build(settings.SiteTitle, settings.MaintenanceMessage);
        await context.Response.WriteAsync(html);
    }

    private static bool IsExemptPath(PathString path)
    {
        if (path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        if (path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/themes", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
