using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Core.Helpers;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Web.Services;

public class DataSeeder : IDataSeeder
{
    private const string DefaultPassword = "Admin123!*";

    private readonly AppDbContext _context;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IMemoryCache _memoryCache;

    public DataSeeder(
        AppDbContext context,
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        ISiteSettingsService siteSettingsService,
        IMemoryCache memoryCache)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _siteSettingsService = siteSettingsService;
        _memoryCache = memoryCache;
    }

    public async Task EnsureInfrastructureAsync()
    {
        string[] roles = { "Admin", "Editör", "Yazar" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new AppRole { Name = role, Description = $"{role} yetkileri." });
        }

        if (!await _context.SiteSettings.AnyAsync())
        {
            await _context.SiteSettings.AddAsync(CreateDefaultSiteSettings());
            await _context.SaveChangesAsync();
        }

        var users = new List<(AppUser User, string Role)>
        {
            (new AppUser { UserName = "admin@blog.com", Email = "admin@blog.com", FirstName = "Sistem", LastName = "Yöneticisi", EmailConfirmed = true }, "Admin"),
            (new AppUser { UserName = "editor@blog.com", Email = "editor@blog.com", FirstName = "İçerik", LastName = "Editörü", EmailConfirmed = true }, "Editör"),
            (new AppUser { UserName = "yazar@blog.com", Email = "yazar@blog.com", FirstName = "Ahmet", LastName = "Yılmaz", EmailConfirmed = true }, "Yazar")
        };

        foreach (var u in users)
        {
            if (await _userManager.FindByEmailAsync(u.User.Email!) == null)
            {
                var result = await _userManager.CreateAsync(u.User, DefaultPassword);
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(u.User, u.Role);
            }
        }
    }

    public async Task SeedDemoContentAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        await InsertDemoContentAsync();
    }

    public async Task ResetToDemoAsync()
    {
        await ClearContentDataAsync();
        await ResetSiteSettingsAsync();
        await InsertDemoContentAsync();
        ClearCaches();
    }

    private async Task ClearContentDataAsync()
    {
        await _context.Comments.ExecuteDeleteAsync();
        await _context.PostTags.ExecuteDeleteAsync();
        await _context.PostImages.ExecuteDeleteAsync();
        await _context.Posts.ExecuteDeleteAsync();
        await _context.Pages.ExecuteDeleteAsync();
        await _context.ContactMessages.ExecuteDeleteAsync();
        await _context.Categories.ExecuteDeleteAsync();
        await _context.Tags.ExecuteDeleteAsync();
    }

    private async Task ResetSiteSettingsAsync()
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            await _context.SiteSettings.AddAsync(CreateDefaultSiteSettings());
        }
        else
        {
            var defaults = CreateDefaultSiteSettings();
            setting.SiteTitle = defaults.SiteTitle;
            setting.SiteDescription = defaults.SiteDescription;
            setting.ActiveTheme = defaults.ActiveTheme;
            setting.LogoPath = null;
            setting.FaviconPath = null;
            setting.ContactEmail = null;
            setting.ContactPhone = null;
            setting.ContactAddress = null;
            setting.FacebookUrl = null;
            setting.InstagramUrl = null;
            setting.GithubUrl = null;
            setting.LinkedinUrl = null;
            setting.GoogleAnalyticsCode = null;
            setting.MapUrl = null;
            setting.WorkingHours = null;
            setting.AdsenseCode = null;
            setting.SidebarAdCode = null;
            setting.PostBottomAdCode = null;
            setting.HomeListAdCode = null;
            setting.AdsTxtContent = null;
            setting.RobotsTxtContent = null;
            setting.IsActive = true;
            setting.UpdatedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }

    private async Task InsertDemoContentAsync()
    {
        var categoryData = new[]
        {
            ("Teknoloji", "Teknoloji dünyasından haberler ve incelemeler."),
            ("Yazılım", "Yazılım geliştirme, framework ve araçlar."),
            ("İş Dünyası", "Girişimcilik, yönetim ve kariyer."),
            ("Kişisel Gelişim", "Verimlilik, motivasyon ve öğrenme."),
            ("Sağlık", "Sağlıklı yaşam ve wellness ipuçları.")
        };

        var categories = categoryData.Select(c => new Category
        {
            Name = c.Item1,
            Slug = SlugHelper.MakeSlug(c.Item1),
            MetaDescription = c.Item2,
            IsActive = true,
            CreatedDate = DateTime.Now
        }).ToList();

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        var tagNames = new[] { "ASP.NET", "C#", "JavaScript", "React", "Veritabanı", "SEO", "Bulut", "Güvenlik", "Mobil", "DevOps" };
        var tags = tagNames.Select(name => new Tag
        {
            Name = name,
            Slug = SlugHelper.MakeSlug(name),
            IsActive = true,
            CreatedDate = DateTime.Now
        }).ToList();

        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        var pages = new[]
        {
            ("Hakkımızda", "hakkimizda", "<p>SimpleBlog, modern ve kullanıcı dostu bir blog yönetim sistemidir.</p>"),
            ("Gizlilik Politikası", "gizlilik-politikasi", "<p>Kişisel verileriniz gizlilik politikamız kapsamında korunmaktadır.</p>"),
            ("Kullanım Koşulları", "kullanim-kosullari", "<p>Siteyi kullanarak kullanım koşullarını kabul etmiş sayılırsınız.</p>")
        }.Select(p => new Page
        {
            Title = p.Item1,
            Slug = p.Item2,
            Content = p.Item3,
            IsActive = true,
            CreatedDate = DateTime.Now
        }).ToList();

        await _context.Pages.AddRangeAsync(pages);
        await _context.SaveChangesAsync();

        var adminId = (await _userManager.FindByEmailAsync("admin@blog.com"))!.Id;
        var yazarId = (await _userManager.FindByEmailAsync("yazar@blog.com"))!.Id;
        var authorIds = new[] { adminId, yazarId };

        var postTitles = new[]
        {
            "Yapay Zeka ile İçerik Üretimi",
            "Modern Web Uygulamalarında Performans",
            "Bulut Bilişime Geçiş Rehberi",
            "Veritabanı Optimizasyon İpuçları",
            "Girişimciler İçin Dijital Pazarlama",
            "Uzaktan Çalışmada Verimlilik",
            "Sağlıklı Yaşam İçin 10 Alışkanlık",
            "Yazılım Test Stratejileri",
            "API Tasarımında En İyi Uygulamalar",
            "Mobil Uygulama Geliştirme Trendleri"
        };

        var posts = new List<Post>();
        var random = new Random(42);

        for (int i = 1; i <= 100; i++)
        {
            var titleBase = postTitles[(i - 1) % postTitles.Length];
            var title = i <= postTitles.Length ? titleBase : $"{titleBase} #{i}";
            var category = categories[(i - 1) % categories.Count];

            posts.Add(new Post
            {
                Title = title,
                Slug = SlugHelper.MakeSlug($"{title}-{i}"),
                Content = $"<p>{title} hakkında detaylı bir blog yazısı. Bu içerik demo amaçlı oluşturulmuştur.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.</p>",
                CategoryId = category.Id,
                AppUserId = authorIds[i % authorIds.Length],
                ViewCount = random.Next(50, 2500),
                Status = PostStatus.Published,
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-random.Next(1, 365)),
                CoverImagePath = $"https://picsum.photos/seed/simpleblog{i}/800/400"
            });
        }

        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        var postTags = new List<PostTag>();
        for (int i = 0; i < posts.Count; i++)
        {
            postTags.Add(new PostTag { PostId = posts[i].Id, TagId = tags[i % tags.Count].Id });
            if (i % 3 == 0)
                postTags.Add(new PostTag { PostId = posts[i].Id, TagId = tags[(i + 1) % tags.Count].Id });
        }

        await _context.PostTags.AddRangeAsync(postTags);
        await _context.SaveChangesAsync();

        var commentAuthors = new[]
        {
            ("Ali Veli", "ali@example.com"),
            ("Ayşe Demir", "ayse@example.com"),
            ("Mehmet Kaya", "mehmet@example.com"),
            ("Zeynep Arslan", "zeynep@example.com"),
            ("Can Öztürk", "can@example.com")
        };

        var comments = new List<Comment>();
        for (int i = 0; i < 30; i++)
        {
            var author = commentAuthors[i % commentAuthors.Length];
            comments.Add(new Comment
            {
                Name = author.Item1,
                Email = author.Item2,
                Content = "Harika bir yazı, teşekkürler!",
                PostId = posts[i % posts.Count].Id,
                IsApproved = i % 4 != 0,
                CreatedDate = DateTime.Now.AddDays(-random.Next(1, 60))
            });
        }

        await _context.Comments.AddRangeAsync(comments);
        await _context.SaveChangesAsync();
    }

    private static SiteSetting CreateDefaultSiteSettings() => new()
    {
        SiteTitle = "SimpleBlog",
        SiteDescription = "Yazılım, teknoloji ve iş dünyasına dair güncel içerikler.",
        ActiveTheme = ThemeService.DefaultTheme,
        IsActive = true
    };

    private void ClearCaches()
    {
        _siteSettingsService.InvalidateCache();

        foreach (var key in new[]
        {
            "Post_GetAll", "Category_GetAll", "Tag_GetAll", "Page_GetAll",
            "Comment_GetAll", "ContactMessage_GetAll", "PostsWithCategory_All"
        })
        {
            _memoryCache.Remove(key);
        }
    }
}
