using Blog.Core.Entities;
using Blog.Data.Context;
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

            // 1. Veritabanı yoksa oluşturur, varsa eksik migration'ları (AppUserId dahil) uygular
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

            // 3. KULLANICILARI OLUŞTUR
            // Her ihtimale karşı tüm Identity kurallarından geçen güçlü bir şifre kullanıyoruz
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
                    else
                    {
                        // EĞER KULLANICI OLUŞMAZSA SEBEBİNİ KONSOLA YAZDIR
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{u.User.Email} oluşturulamadı. Hatalar:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Description}");
                        }
                        Console.ResetColor();
                    }
                }
            }

            // 4. KATEGORİLERİ OLUŞTUR
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "ASP.NET Core", CreatedDate = DateTime.Now, IsActive = true, IsDeleted = false },
                    new Category { Name = "Flutter", CreatedDate = DateTime.Now, IsActive = true, IsDeleted = false },
                    new Category { Name = "Liderlik & Kariyer", CreatedDate = DateTime.Now, IsActive = true, IsDeleted = false }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 5. DEMO POSTLARI (YAZILARI) OLUŞTUR
            if (!await context.Posts.AnyAsync())
            {
                var aspNetKategori = await context.Categories.FirstOrDefaultAsync(c => c.Name == "ASP.NET Core");
                var flutterKategori = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Flutter");
                var kariyerKategori = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Liderlik & Kariyer");

                var adminUser = await userManager.FindByEmailAsync("admin@blog.com");
                var yazarUser = await userManager.FindByEmailAsync("yazar1@blog.com");

                // Null referans hatası almamak için güvenlik kontrolü
                if (aspNetKategori != null && flutterKategori != null && kariyerKategori != null && adminUser != null && yazarUser != null)
                {
                    var posts = new List<Post>
                    {
                        new Post
                        {
                            Title = "ASP.NET Core ile Temiz Mimari (Clean Architecture)",
                            Content = "<h2>Temiz Mimari Neden Önemlidir?</h2><p>Projeler büyüdükçe kodun yönetilebilirliği zorlaşır. Katmanlı mimari ve <strong>Dependency Injection</strong> kullanarak projelerinizi geleceğe hazır hale getirebilirsiniz.</p><ul><li>Core Katmanı</li><li>Data Katmanı</li><li>Service Katmanı</li></ul><p>Bu yapılar sayesinde veri tabanı bağımlılığını en aza indiririz.</p>",
                            CategoryId = aspNetKategori.Id,
                            AppUserId = adminUser.Id,
                            CoverImagePath = "https://picsum.photos/seed/aspnet/800/400", // RESTGELE GÖRSEL EKLENDİ
                            CreatedDate = DateTime.Now.AddDays(-5),
                            IsActive = true,
                            IsDeleted = false
                        },
                        new Post
                        {
                            Title = "Flutter ile Çapraz Platform Uygulama Geliştirme",
                            Content = "<h2>Tek Kod, İki Platform</h2><p>Mobil uygulama dünyasında <em>Flutter</em> rüzgarı esmeye devam ediyor. Dart dili ile yazılan widget mimarisi sayesinde hem iOS hem de Android için harika arayüzler tasarlamak çok kolay.</p><p>State Management (Durum Yönetimi) konusunda Riverpod veya Provider tercih edilebilir.</p>",
                            CategoryId = flutterKategori.Id,
                            AppUserId = yazarUser.Id,
                            CoverImagePath = "https://picsum.photos/seed/flutter/800/400", // RESTGELE GÖRSEL EKLENDİ
                            CreatedDate = DateTime.Now.AddDays(-2),
                            IsActive = true,
                            IsDeleted = false
                        },
                        new Post
                        {
                            Title = "Başarılı Bir Yazılım Ekip Lideri Olmanın Sırları",
                            Content = "<h2>Ekibi Yönetmek Değil, Yönlendirmek</h2><p>İyi bir yazılım ekip lideri, sadece kod kalitesiyle değil, ekibin motivasyonuyla da ilgilenmelidir.</p><ol><li>Agile metodolojilerini doğru uygulamak</li><li>Kod inceleme (Code Review) süreçlerini adil yürütmek</li><li>Teknik borçları zamanında ödemek</li></ol><blockquote><p>\"İyi liderler başarıyı paylaşır, başarısızlığı sahiplenir.\"</p></blockquote>",
                            CategoryId = kariyerKategori.Id,
                            AppUserId = yazarUser.Id,
                            CoverImagePath = "https://picsum.photos/seed/kariyer/800/400", // RESTGELE GÖRSEL EKLENDİ
                            CreatedDate = DateTime.Now.AddDays(-1),
                            IsActive = true,
                            IsDeleted = false
                        }
                    };
                    await context.Posts.AddRangeAsync(posts);
                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veritabanı başlatılırken kritik bir hata oluştu: {ex.Message}");
        }
    }
}