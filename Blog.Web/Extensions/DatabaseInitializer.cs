using Blog.Core.Entities;
using Blog.Core.Enums;
using Blog.Data.Context;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Extensions;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            // 1. Veritabanı yoksa oluşturur, varsa eksik migration'ları uygular
            await context.Database.MigrateAsync();
            Console.WriteLine("Veritabanı başarıyla ayağa kaldırıldı.");

            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // 2. ROLLERİ OLUŞTUR
            string[] roles = { "Admin", "Editör", "Yazar" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole { Name = role, Description = $"{role} yetkileri." });
                }
            }

            // 3. KULLANICILARI OLUŞTUR (Senin orijinal güvenli yapın)
            var defaultPassword = "Admin123!*";
            var users = new List<(AppUser User, string Role)>
            {
                (new AppUser { UserName = "admin@blog.com", Email = "admin@blog.com", FirstName = "Sistem", LastName = "Yöneticisi", EmailConfirmed = true }, "Admin"),
                (new AppUser { UserName = "editor@blog.com", Email = "editor@blog.com", FirstName = "İçerik", LastName = "Editörü", EmailConfirmed = true }, "Editör"),
                (new AppUser { UserName = "yazar1@blog.com", Email = "yazar1@blog.com", FirstName = "Ahmet", LastName = "Yılmaz", EmailConfirmed = true }, "Yazar")
            };

            foreach (var u in users)
            {
                if (await userManager.FindByEmailAsync(u.User.Email) == null)
                {
                    var result = await userManager.CreateAsync(u.User, defaultPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(u.User, u.Role);
                        Console.WriteLine($"{u.User.Email} kullanıcısı başarıyla oluşturuldu.");
                    }
                }
            }

            // --- BUNDAN SONRASI BOGUS İLE 1000'ER ADET STRESS TESTİ VERİSİ ---

            // Eğer veritabanında halihazırda kategori varsa tohumlamayı atla (her açılışta 1000 tane daha eklemesin)
            if (!await context.Categories.AnyAsync())
            {
                Console.WriteLine("Bogus ile sahte veriler üretiliyor. Lütfen bekleyin...");

                var faker = new Faker("tr");

                // 4. 1000 ADET KATEGORİ
                var categories = new Faker<Category>("tr")
                    .RuleFor(c => c.Name, f => f.Commerce.Department() + " " + f.IndexGlobal)
                    .RuleFor(c => c.Slug, (f, c) => f.Lorem.Slug())
                    .RuleFor(c => c.IsActive, true)
                    .RuleFor(c => c.CreatedDate, f => f.Date.Past(1))
                    .Generate(1000);

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // 5. 1000 ADET ETİKET
                var tags = new Faker<Tag>("tr")
                    .RuleFor(t => t.Name, f => f.Commerce.ProductAdjective() + " " + f.IndexGlobal)
                    .RuleFor(t => t.Slug, (f, t) => f.Lorem.Slug())
                    .RuleFor(t => t.IsActive, true)
                    .RuleFor(t => t.CreatedDate, f => f.Date.Past(1))
                    .Generate(1000);

                await context.Tags.AddRangeAsync(tags);
                await context.SaveChangesAsync();

                // 6. 1000 ADET SABİT SAYFA
                var pages = new Faker<Page>("tr")
                    .RuleFor(p => p.Title, f => f.Lorem.Sentence(3))
                    .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(5))
                    .RuleFor(p => p.Slug, (f, p) => f.Lorem.Slug())
                    .RuleFor(p => p.IsActive, true)
                    .RuleFor(p => p.CreatedDate, f => f.Date.Past(1))
                    .Generate(1000);

                await context.Pages.AddRangeAsync(pages);
                await context.SaveChangesAsync();

                // 7. 1000 ADET YAZI (Rastgele Kategori ve Yazar ile)
                var categoryIds = categories.Select(c => c.Id).ToList();
                var userIds = new List<string>
                {
                    (await userManager.FindByEmailAsync("admin@blog.com")).Id,
                    (await userManager.FindByEmailAsync("yazar1@blog.com")).Id
                };

                var posts = new Faker<Post>("tr")
                    .RuleFor(p => p.Title, f => f.Lorem.Sentence(5))
                    .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(8))
                    .RuleFor(p => p.Slug, (f, p) => f.Lorem.Slug())
                    .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
                    .RuleFor(p => p.AppUserId, f => f.PickRandom(userIds))
                    .RuleFor(p => p.ViewCount, f => f.Random.Number(10, 5000))
                    .RuleFor(p => p.Status, PostStatus.Published)
                    .RuleFor(p => p.CreatedDate, f => f.Date.Past(1))
                    .RuleFor(p => p.CoverImagePath, f => $"https://picsum.photos/seed/{f.Random.AlphaNumeric(10)}/800/400")
                    .Generate(1000);

                await context.Posts.AddRangeAsync(posts);
                await context.SaveChangesAsync();

                // 8. 1000 ADET YORUM
                var postIds = posts.Select(p => p.Id).ToList();
                var comments = new Faker<Comment>("tr")
                    .RuleFor(c => c.Name, f => f.Name.FullName())
                    .RuleFor(c => c.Email, f => f.Internet.Email())
                    .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
                    .RuleFor(c => c.PostId, f => f.PickRandom(postIds))
                    .RuleFor(c => c.IsApproved, f => f.Random.Bool())
                    .RuleFor(c => c.CreatedDate, f => f.Date.Past(1))
                    .Generate(1000);

                await context.Comments.AddRangeAsync(comments);
                await context.SaveChangesAsync();

                Console.WriteLine("Tüm sahte veriler başarıyla veritabanına işlendi!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veritabanı başlatılırken kritik bir hata oluştu: {ex.Message}");
        }
    }
}