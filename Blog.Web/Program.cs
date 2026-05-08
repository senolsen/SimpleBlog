using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Blog.Service.Concrete;
using Blog.Web.Extensions;
using Blog.Web.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC Servisleri
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
// 2. Veritabaný (DbContext) ve DB Ayarý
// appsettings.json'dan aktif veritabaný türünü okuyoruz
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
            // Pomelo MySQL için sunucu versiyonunu otomatik algýlama
            options.UseMySql(mySqlConn, ServerVersion.AutoDetect(mySqlConn));
            break;
        case "Sqlite":
            // ÞÝFRELÝ MOTORU BAÞLATIYORUZ (ÇOK ÖNEMLÝ)
            Batteries_V2.Init();
            options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
            break;

        default:
            throw new Exception($"Desteklenmeyen veritabaný saðlayýcýsý: {activeProvider}");
    }
});

// 3. Identity Ayarlarý (Özel AppUser ve AppRole sýnýflarýmýzla)
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true; // '**' gibi karakterler için

    // YENÝ EKLENEN KÝLÝTLEME AYARLARI
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // 15 dakika kilitle
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 yanlýþ denemede kilitle
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4. Cookie (Çerez) ve Yetkilendirme Yönlendirmeleri
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Auth/Login"; // Giriþ yapmamýþ kiþi buraya atýlýr
    options.AccessDeniedPath = "/Admin/Auth/AccessDenied"; // Yetkisi (Rolü) yetmeyen buraya atýlýr
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Oturum süresi
    options.SlidingExpiration = true;
});

// 5. Service Katmaný Baðýmlýlýklarý (Dependency Injection - IoC)

// YENÝ EKLENEN DATA KATMANI KAYITLARI
builder.Services.AddScoped<Blog.Data.UnitOfWorks.IUnitOfWork, Blog.Data.UnitOfWorks.UnitOfWork>();
builder.Services.AddScoped(typeof(Blog.Data.Repositories.Abstract.IGenericRepository<>), typeof(Blog.Data.Repositories.Concrete.GenericRepository<>));
builder.Services.AddScoped<Blog.Data.Repositories.Abstract.IPostRepository, Blog.Data.Repositories.Concrete.PostRepository>();

// MEVCUT SERVÝS KAYITLARI
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericManager<>));
builder.Services.AddScoped<IPostService, PostManager>();

var app = builder.Build();

// 6. Otomatik Migration ve Seed Data (Kendi yazdýðýmýz Extension)
// Uygulama HTTP isteklerini karþýlamadan önce veritabanýný hazýr hale getirir
await app.InitializeDatabaseAsync();

// 7. HTTP Request Pipeline (Ara Yazýlýmlar / Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware sýralamasý çok önemlidir, bu sýra bozulmamalýdýr
app.UseRouting();
app.UseMiddleware<AdminLicenseMiddleware>();
app.UseAuthentication(); // Önce kimlik doðrulanýr (Giriþ yapmýþ mý?)
app.UseAuthorization();  // Sonra yetki kontrol edilir (Admin mi?)

// 1. Önce Admin Paneli (Area) Rotalarý
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Sonra Sitenin Standart Rotalarý (Home, Post, Login vs.)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 3. EN ALTTA: CATCH-ALL (Her Þeyi Yakalayan) Fallback Rotasý
// Eðer yukarýdaki hiçbir þarta uymazsa buraya düþecek!
app.MapControllerRoute(
    name: "DynamicPage",
    pattern: "{slug}",
    defaults: new { controller = "Page", action = "Detail" });

app.Run();

app.Run();