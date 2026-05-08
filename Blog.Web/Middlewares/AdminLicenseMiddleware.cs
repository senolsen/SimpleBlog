using Blog.Core.Helpers;
using Blog.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Web.Middlewares;

public class AdminLicenseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public AdminLicenseMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Eğer istek lisans giriş sayfasına gidiyorsa, kontrol etmeden geçişe izin ver!
        if (context.Request.Path.StartsWithSegments("/Admin/License"))
        {
            await _next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/Admin"))
        {
            var currentDomain = context.Request.Host.Host.ToLower();
            if (currentDomain == "localhost") { await _next(context); return; }

            if (!_cache.TryGetValue("IsLicenseValid", out bool isValid))
            {
                // DB'ye erişmek için context'i buradan alıyoruz
                using (var scope = context.RequestServices.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Veritabanındaki aktif lisansı getir
                    var license = await db.Licenses.FirstOrDefaultAsync(l => l.IsActive && !l.IsDeleted);

                    if (license == null || string.IsNullOrEmpty(license.Key))
                    {
                        isValid = false;
                    }
                    else
                    {
                        var decryptedDomain = SecurityHelper.DecryptDomain(license.Key);
                        isValid = decryptedDomain == currentDomain;
                    }
                }

                _cache.Set("IsLicenseValid", isValid, TimeSpan.FromHours(12));
            }

            if (!isValid)
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("<h1 style='color:red; text-align:center;'>Lisans Hatası!</h1><p style='text-align:center;'>Geçerli bir lisans bulunamadı.</p>");
                return;
            }
        }
        await _next(context);
    }
}