using System.Text;
using System.Xml;
using Blog.Core.Enums;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Controllers;

public class RssController : Controller
{
    private readonly IPostService _postService;
    private readonly ISiteSettingsService _siteSettingsService;

    public RssController(IPostService postService, ISiteSettingsService siteSettingsService)
    {
        _postService = postService;
        _siteSettingsService = siteSettingsService;
    }

    [Route("feed")]
    [Route("rss.xml")]
    [Produces("application/rss+xml")]
    public async Task<IActionResult> Index()
    {
        var settings = await _siteSettingsService.GetSettingsAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var siteTitle = settings.SiteTitle ?? "SimpleBlog";
        var siteDescription = settings.SiteDescription ?? "Blog yazıları";

        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var posts = allPosts
            .Where(p => p.Status == PostStatus.Published && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .Take(50)
            .ToList();

        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");

            writer.WriteStartElement("channel");
            writer.WriteElementString("title", siteTitle);
            writer.WriteElementString("link", baseUrl);
            writer.WriteElementString("description", siteDescription);
            writer.WriteElementString("language", "tr");
            writer.WriteElementString("lastBuildDate", DateTime.Now.ToString("R"));

            writer.WriteStartElement("atom", "link", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("href", $"{baseUrl}/feed");
            writer.WriteAttributeString("rel", "self");
            writer.WriteAttributeString("type", "application/rss+xml");
            writer.WriteEndElement();

            foreach (var post in posts)
            {
                writer.WriteStartElement("item");
                writer.WriteElementString("title", post.Title);
                writer.WriteElementString("link", $"{baseUrl}/Makale/{post.Slug}");
                writer.WriteElementString("guid", $"{baseUrl}/Makale/{post.Slug}");
                writer.WriteElementString("pubDate", post.CreatedDate.ToString("R"));

                var description = post.MetaDescription
                    ?? System.Text.RegularExpressions.Regex.Replace(post.Content ?? "", "<[^>]+>", "").Trim();
                if (description.Length > 300)
                    description = description[..300] + "...";

                writer.WriteElementString("description", description);

                if (post.Category != null)
                    writer.WriteElementString("category", post.Category.Name);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Content(sb.ToString(), "application/rss+xml", Encoding.UTF8);
    }
}
