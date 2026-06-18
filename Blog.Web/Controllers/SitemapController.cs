using System.Text;
using Blog.Core.Entities;
using Blog.Core.Enums; // ENUM KULLANIMI İÇİN EKLENDİ
using Blog.Data.Context;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Controllers;

// Bu controller'ın Area'sı yoktur, tüm kullanıcılara ve arama motorlarına açıktır.
public class SitemapController : Controller
{
    private readonly IGenericService<Post> _postService;
    private readonly IGenericService<Category> _categoryService;
    private readonly AppDbContext _context;

    public SitemapController(IGenericService<Post> postService, IGenericService<Category> categoryService, AppDbContext context)
    {
        _postService = postService;
        _categoryService = categoryService;
        _context = context;
    }

    [Route("sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // 1. ANA SAYFA
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</lastmod>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>1.0</priority>");
        sb.AppendLine("  </url>");

        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/feed</loc>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>0.5</priority>");
        sb.AppendLine("  </url>");

        // 2. SABİT SAYFALAR (YENİ EKLENEN KISIM)
        // Veritabanından aktif ve silinmemiş sayfaları çekiyoruz
        var pages = await _context.Pages
                                  .Where(p => p.IsActive && !p.IsDeleted)
                                  .ToListAsync();

        foreach (var page in pages)
        {
            sb.AppendLine("  <url>");
            // Sayfa slug'ını kullanarak doğrudan domain.com/slug URL'sini oluşturuyoruz
            sb.AppendLine($"    <loc>{baseUrl}/{page.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{page.UpdatedDate?.ToString("yyyy-MM-ddTHH:mm:sszzz") ?? page.CreatedDate.ToString("yyyy-MM-ddTHH:mm:sszzz")}</lastmod>");
            sb.AppendLine("    <changefreq>monthly</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>"); // Sayfalar için 0.8 idealdir
            sb.AppendLine("  </url>");
        }

        // 3. KATEGORİLER (Mevcut kodların)
        var categories = await _categoryService.WhereAsync(c => c.IsActive && !c.IsDeleted);
        foreach (var category in categories)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/Kategori/{category.Slug}</loc>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.7</priority>");
            sb.AppendLine("  </url>");
        }

        // 4. MAKALELER / POSTLAR (Mevcut kodların)
        var posts = await _postService.WhereAsync(p => p.Status == PostStatus.Published && !p.IsDeleted);
        foreach (var post in posts)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/Makale/{post.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{post.UpdatedDate?.ToString("yyyy-MM-ddTHH:mm:sszzz") ?? post.CreatedDate.ToString("yyyy-MM-ddTHH:mm:sszzz")}</lastmod>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.9</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    // domain.com/robots.txt adresinde çalışır
    [Route("robots.txt")]
    public async Task<IActionResult> RobotsTxt()
    {
        var setting = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        if (!string.IsNullOrWhiteSpace(setting?.RobotsTxtContent))
            return Content(setting.RobotsTxtContent.Trim(), "text/plain", Encoding.UTF8);

        var content = $"User-agent: *{Environment.NewLine}Allow: /{Environment.NewLine}Disallow: /Admin/{Environment.NewLine}{Environment.NewLine}Sitemap: {baseUrl}/sitemap.xml";
        return Content(content, "text/plain", Encoding.UTF8);
    }

    // domain.com/ads.txt adresinde çalışır
    [Route("ads.txt")]
    public async Task<IActionResult> AdsTxt()
    {
        // Doğrudan AppDbContext üzerinden SiteSettings tablosuna ulaşıyoruz
        var setting = await _context.SiteSettings.FirstOrDefaultAsync();

        string content = setting?.AdsTxtContent ?? "";

        return Content(content, "text/plain", Encoding.UTF8);
    }
}

