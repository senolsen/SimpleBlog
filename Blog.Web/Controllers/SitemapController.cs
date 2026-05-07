using System.Text;
using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

// Bu controller'ın Area'sı yoktur, tüm kullanıcılara ve arama motorlarına açıktır.
public class SitemapController : Controller
{
    private readonly IGenericService<Post> _postService;
    private readonly IGenericService<Category> _categoryService;

    public SitemapController(IGenericService<Post> postService, IGenericService<Category> categoryService)
    {
        _postService = postService;
        _categoryService = categoryService;
    }

    // domain.com/sitemap.xml adresinde çalışır
    [Route("sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var posts = await _postService.WhereAsync(p => !p.IsDeleted && p.IsActive);
        var categories = await _categoryService.WhereAsync(c => !c.IsDeleted);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // 1. Ana Sayfa
        sb.AppendLine($"<url><loc>{baseUrl}/</loc><priority>1.0</priority><changefreq>daily</changefreq></url>");

        // 2. Kategoriler (Öncelik: 0.8)
        foreach (var category in categories)
        {
            sb.AppendLine("<url>");
            sb.AppendLine($"<loc>{baseUrl}/Category/{category.Slug}</loc>");
            sb.AppendLine("<priority>0.8</priority>");
            sb.AppendLine("<changefreq>weekly</changefreq>");
            sb.AppendLine("</url>");
        }

        // 3. Makaleler (Öncelik: 0.9)
        foreach (var post in posts)
        {
            sb.AppendLine("<url>");
            sb.AppendLine($"<loc>{baseUrl}/Post/{post.Slug}</loc>");
            // Makale güncellendiyse güncelleme tarihini, yoksa oluşturulma tarihini SEO'ya bildiriyoruz
            sb.AppendLine($"<lastmod>{(post.UpdatedDate?.ToString("yyyy-MM-dd") ?? post.CreatedDate.ToString("yyyy-MM-dd"))}</lastmod>");
            sb.AppendLine("<priority>0.9</priority>");
            sb.AppendLine("<changefreq>monthly</changefreq>");
            sb.AppendLine("</url>");
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    // domain.com/robots.txt adresinde çalışır
    [Route("robots.txt")]
    public IActionResult RobotsTxt()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();

        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /Admin/"); // Arama motorlarının admin paneline girmesini yasakla!
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml"); // Google'a sitemap adresimizi göster

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }
}