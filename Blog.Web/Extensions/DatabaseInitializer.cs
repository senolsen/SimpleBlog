using Blog.Data.Context;
using Blog.Web.Services;
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

            if (!await context.Database.CanConnectAsync())
                Console.WriteLine("Veritabanı bulunamadı, oluşturuluyor...");

            await context.Database.MigrateAsync();
            Console.WriteLine("Veritabanı migration'ları başarıyla uygulandı.");

            var seeder = services.GetRequiredService<IDataSeeder>();
            await seeder.EnsureInfrastructureAsync();
            await seeder.SeedDemoContentAsync();

            Console.WriteLine("Veritabanı başlatma işlemi tamamlandı.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Veritabanı başlatılırken kritik bir hata oluştu: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Detay: {ex.InnerException.Message}");
            throw;
        }
    }
}
