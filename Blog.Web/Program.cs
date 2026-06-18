using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Blog.Service.Concrete;
using Blog.Web.Extensions;
using Blog.Web.Filters;
using Blog.Web.Middlewares;
using Blog.Web.Models;
using Blog.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC Servisleri
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SiteLayoutFilter>();
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
// 2. Veritaban� (DbContext) ve DB Ayar�
// appsettings.json'dan aktif veritaban� t�r�n� okuyoruz
var activeProvider = builder.Configuration["DatabaseSettings:ActiveProvider"];
builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (activeProvider)
    {
        case "SqlServer":
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
            break;

        case "MySql":
           
            var mySqlConn = builder.Configuration.GetConnectionString("MySqlConnection");
            // Pomelo MySQL i�in sunucu versiyonunu otomatik alg�lama
            options.UseMySql(mySqlConn, ServerVersion.AutoDetect(mySqlConn));
            break;
        case "Sqlite":
            // ��FREL� MOTORU BA�LATIYORUZ (�OK �NEML�)
            Batteries_V2.Init();
            options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
            break;

        default:
            throw new Exception($"Desteklenmeyen veritaban� sa�lay�c�s�: {activeProvider}");
    }
});

// 3. Identity Ayarlar� (�zel AppUser ve AppRole s�n�flar�m�zla)
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true; // '**' gibi karakterler i�in

    // YEN� EKLENEN K�L�TLEME AYARLARI
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // 15 dakika kilitle
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 yanl�� denemede kilitle
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4. Cookie (�erez) ve Yetkilendirme Y�nlendirmeleri
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Auth/Login"; // Giri� yapmam�� ki�i buraya at�l�r
    options.AccessDeniedPath = "/Admin/Auth/AccessDenied"; // Yetkisi (Rol�) yetmeyen buraya at�l�r
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Oturum s�resi
    options.SlidingExpiration = true;
});

// 5. Service Katman� Ba��ml�l�klar� (Dependency Injection - IoC)

// YEN� EKLENEN DATA KATMANI KAYITLARI
builder.Services.AddScoped<Blog.Data.UnitOfWorks.IUnitOfWork, Blog.Data.UnitOfWorks.UnitOfWork>();
builder.Services.AddScoped(typeof(Blog.Data.Repositories.Abstract.IGenericRepository<>), typeof(Blog.Data.Repositories.Concrete.GenericRepository<>));
builder.Services.AddScoped<Blog.Data.Repositories.Abstract.IPostRepository, Blog.Data.Repositories.Concrete.PostRepository>();

// MEVCUT SERV�S KAYITLARI
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericManager<>));
builder.Services.AddScoped<IPostService, PostManager>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsManager>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IDataSeeder, DataSeeder>();
builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddScoped<IFtpDeployService, FtpDeployService>();
builder.Services.AddScoped<IApplicationRestartService, ApplicationRestartService>();
builder.Services.AddSingleton<IFtpCredentialProtector, FtpCredentialProtector>();

builder.Services.Configure<LicenseSettings>(builder.Configuration.GetSection("LicenseSettings"));
builder.Services.Configure<UpdateSettings>(builder.Configuration.GetSection("UpdateSettings"));

builder.Services.AddHttpClient("GitHubUpdates", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SimpleBlog-Updater/1.0");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// 6. Otomatik Migration ve Seed Data (Kendi yazd���m�z Extension)
// Uygulama HTTP isteklerini kar��lamadan �nce veritaban�n� haz�r hale getirir
await app.InitializeDatabaseAsync();

// 7. HTTP Request Pipeline (Ara Yaz�l�mlar / Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware s�ralamas� �ok �nemlidir, bu s�ra bozulmamal�d�r
app.UseRouting();
app.UseMiddleware<MaintenanceMiddleware>();
app.UseMiddleware<AdminLicenseMiddleware>();
app.UseAuthentication(); // �nce kimlik do�rulan�r (Giri� yapm�� m�?)
app.UseAuthorization();  // Sonra yetki kontrol edilir (Admin mi?)

// 1. �nce Admin Paneli (Area) Rotalar�
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Sonra Sitenin Standart Rotalar� (Home, Post, Login vs.)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 3. EN ALTTA: CATCH-ALL (Her �eyi Yakalayan) Fallback Rotas�
// E�er yukar�daki hi�bir �arta uymazsa buraya d��ecek!
app.MapControllerRoute(
    name: "DynamicPage",
    pattern: "{slug}",
    defaults: new { controller = "Page", action = "Detail" });

app.Run();