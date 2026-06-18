using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Data.Context; // AppDbContext için eklendi
using Blog.Service.Abstract;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Include ve ToListAsync için eklendi

namespace Blog.Web.Controllers;

public class MakaleController : Controller
{
    private readonly IPostService _postService;
    private readonly IGenericService<Comment> _commentService;
    private readonly AppDbContext _context;
    private readonly IThemeService _themeService;

    public MakaleController(IPostService postService, IGenericService<Comment> commentService, AppDbContext context, IThemeService themeService)
    {
        _postService = postService;
        _commentService = commentService;
        _context = context;
        _themeService = themeService;
    }

    [Route("Makale/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return RedirectToAction("Index", "Home");

        // 1. Makaleyi çekiyoruz
        var allPosts = await _postService.GetPostsWithCategoryAsync(null);
        var post = allPosts.FirstOrDefault(p => p.Slug == slug && p.Status == PostStatus.Published && !p.IsDeleted);

        if (post == null)
            return RedirectToAction("Index", "Home");

        // 2. Yorumları çekip nesneye atıyoruz
        var approvedComments = await _commentService.WhereAsync(c => c.PostId == post.Id && c.IsApproved && !c.IsDeleted);
        post.Comments = approvedComments.ToList();

        // 3. YENİ EKLENEN: Bu makaleye ait etiketleri (Tag) ara tablodan çekip nesneye bağlıyoruz
        post.PostTags = await _context.PostTags
            .Include(pt => pt.Tag)
            .Where(pt => pt.PostId == post.Id)
            .ToListAsync();

        // Okunma sayısını 1 artır ve veritabanına kaydet
        post.ViewCount += 1;
        await _postService.UpdateAsync(post);

        return View(_themeService.GetViewPath("Makale/Details"), post);
    }

    [HttpPost]
    [Route("Makale/AddComment")]
    public async Task<IActionResult> AddComment(int postId, string slug, string name, string email, string content)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Lütfen adınızı ve yorumunuzu eksiksiz giriniz.";
            return Redirect($"/Makale/{slug}");
        }

        var newComment = new Comment
        {
            PostId = postId,
            Name = name,
            Email = email ?? string.Empty,
            Content = content,
            CreatedDate = DateTime.Now,
            IsApproved = false // İlk eklendiğinde onay bekler
        };

        await _commentService.AddAsync(newComment);

        TempData["SuccessMessage"] = "Yorumunuz başarıyla alındı. Editör onayından sonra yayımlanacaktır. Katkınız için teşekkürler!";

        return Redirect($"/Makale/{slug}#comments");
    }
}