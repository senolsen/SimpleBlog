using System.Text;
using System.Xml.Linq;
using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

// Bu sayede site.com/sitemap.xml yazıldığında bu Controller çalışacak
[Route("sitemap.xml")]
public class SitemapController : Controller
{
    private readonly IGenericService<Post> _postService;
    private readonly IGenericService<Category> _categoryService;

    public SitemapController(IGenericService<Post> postService, IGenericService<Category> categoryService)
    {
        _postService = postService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Kendi sitenin canlı URL'sini buraya alacağız (Şimdilik Localhost)
        string baseUrl = $"{Request.Scheme}://{Request.Host}";

        // XML namespace (Google Standartları)
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XElement(xmlns + "urlset");

        // 1. ANA SAYFAYI EKLE
        sitemap.Add(CreateUrlElement(xmlns, baseUrl, DateTime.Now, "1.0", "daily"));

        // 2. KATEGORİLERİ EKLE
        var categories = await _categoryService.WhereAsync(c => c.IsActive && !c.IsDeleted);
        foreach (var category in categories)
        {
            string url = $"{baseUrl}/kategori/{category.Slug}";
            sitemap.Add(CreateUrlElement(xmlns, url, category.CreatedDate, "0.8", "weekly"));
        }

        // 3. YAZILARI (POSTLARI) EKLE
        var posts = await _postService.WhereAsync(p => p.IsActive && !p.IsDeleted);
        foreach (var post in posts)
        {
            string url = $"{baseUrl}/makale/{post.Slug}";
            sitemap.Add(CreateUrlElement(xmlns, url, post.CreatedDate, "0.9", "monthly"));
        }

        // XML formatında yanıt dön (Fiziksel dosya oluşturmadan!)
        var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), sitemap);
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    // Tekrarı önlemek için XML Node oluşturucu yardımcı metod
    private XElement CreateUrlElement(XNamespace xmlns, string url, DateTime lastMod, string priority, string changeFreq)
    {
        return new XElement(xmlns + "url",
            new XElement(xmlns + "loc", url),
            new XElement(xmlns + "lastmod", lastMod.ToString("yyyy-MM-ddTHH:mm:sszzz")),
            new XElement(xmlns + "changefreq", changeFreq),
            new XElement(xmlns + "priority", priority)
        );
    }
}