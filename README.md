# SimpleBlog — Jetexsoft Blog Sistemi v1.0

ASP.NET Core 8 tabanlı, çoklu veritabanı destekli, tema değiştirilebilir blog yönetim sistemi.

## Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server, MySQL veya SQLite (tercihe göre)
- IIS, Kestrel veya Docker ile barındırma

## Hızlı Kurulum

```bash
git clone <repo-url> SimpleBlog
cd SimpleBlog
dotnet restore
dotnet run --project Blog.Web
```

Uygulama ilk çalıştırmada otomatik olarak migration uygular ve demo verileri oluşturur.

- **Site:** `https://localhost:5001` (veya terminalde gösterilen adres)
- **Admin panel:** `/Admin/Auth/Login`

## Veritabanı Yapılandırması

`Blog.Web/appsettings.json` dosyasını düzenleyin:

```json
{
  "DatabaseSettings": {
    "ActiveProvider": "SqlServer"
  },
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=Ablogcms;Trusted_Connection=True;TrustServerCertificate=True;",
    "MySqlConnection": "Server=localhost;Database=Ablogcms;Uid=root;Pwd=sifreniz;",
    "SqliteConnection": "Data Source=App_Data/Ablogcms.db;Password=SuperGizliMusteriSifresi123!;"
  }
}
```

| Provider | Değer |
|----------|-------|
| SQL Server | `"SqlServer"` |
| MySQL (Pomelo) | `"MySql"` |
| SQLite (şifreli) | `"Sqlite"` |

### SQLite Notu

SQLite kullanırken `App_Data` klasörünün yazılabilir olduğundan emin olun. Connection string'deki `Password` değeri veritabanı şifrelemesi içindir.

## Production Ortamı

1. `appsettings.Production.json` dosyasını kendi ortamınıza göre düzenleyin
2. Ortam değişkeni ayarlayın: `ASPNETCORE_ENVIRONMENT=Production`
3. Publish alın:

```bash
dotnet publish Blog.Web -c Release -o ./publish
```

## Demo Kullanıcılar

İlk kurulumda aşağıdaki kullanıcılar otomatik oluşturulur. **Canlı ortamda şifreleri mutlaka değiştirin.**

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Admin | `admin@blog.com` | `Admin123!*` |
| Editör | `editor@blog.com` | `Admin123!*` |
| Yazar | `yazar@blog.com` | `Admin123!*` |

## Demo İçerik

İlk kurulumda (kategori tablosu boşsa) otomatik oluşturulur:

- 5 kategori
- 100 blog yazısı
- 10 etiket
- 3 sabit sayfa (Hakkımızda, Gizlilik Politikası, Kullanım Koşulları)
- 30 yorum

Temiz kurulum için mevcut veritabanını silip uygulamayı yeniden başlatın.

## Lisans Aktivasyonu

Admin paneli production ortamında domain lisansı gerektirir (`localhost` hariç).

1. `GeneratorLicenceCode` projesini çalıştırın:

```bash
dotnet run --project GeneratorLicenceCode
```

2. Müşteri domain adını girin ve üretilen anahtarı kopyalayın
3. Admin panel → **Lisans Yönetimi** (`/Admin/License`) sayfasına anahtarı yapıştırın

## Tema Değiştirme

Admin panel → **Genel Ayarlar** → **Aktif Tema** bölümünden seçim yapın.

Mevcut temalar:

| Tema | Klasör |
|------|--------|
| ZenBlog | `Views/Shared/Themes/ZenBlog/` |
| BlogHome | `Views/Shared/Themes/BlogHome/` |
| MagDesign | `Views/Shared/Themes/MagDesign/` |

Yeni tema eklemek için `Views/Shared/Themes/{TemaAdi}/_Layout.cshtml` dosyası oluşturun ve `@RenderBody()` içermesine dikkat edin. Statik dosyalar `wwwroot/themes/{temaadi}/assets/` altına konur.

## Rol Yetkileri

| Özellik | Admin | Editör | Yazar |
|---------|-------|--------|-------|
| Dashboard | ✓ | ✓ | ✓ (kendi verileri) |
| Yazı yönetimi | ✓ | ✓ | ✓ (kendi yazıları) |
| Kategori / Etiket | ✓ | ✓ | ✗ |
| Yorumlar | ✓ | ✓ | ✗ |
| Sabit sayfalar | ✓ | ✓ | ✗ |
| Gelen mesajlar | ✓ | ✓ | ✗ |
| Kullanıcı yönetimi | ✓ | ✗ | ✗ |
| Site ayarları | ✓ | ✗ | ✗ |
| Lisans yönetimi | ✓ | ✗ | ✗ |

## SEO ve Beslemeler

| Endpoint | Açıklama |
|----------|----------|
| `/sitemap.xml` | Site haritası |
| `/robots.txt` | Arama motoru kuralları |
| `/ads.txt` | AdSense reklam doğrulama |
| `/feed` | RSS 2.0 beslemesi |

## Proje Yapısı

```
SimpleBlog/
├── Blog.Core/          # Entity, enum, helper sınıfları
├── Blog.Data/          # EF Core DbContext, repository, migration
├── Blog.Service/       # İş mantığı servisleri
├── Blog.Web/           # MVC web uygulaması (public + admin)
└── GeneratorLicenceCode/  # Lisans anahtarı üretici
```

## Güvenlik Kontrol Listesi

- [ ] Demo kullanıcı şifrelerini değiştirin
- [ ] `appsettings.Production.json` connection string'lerini güncelleyin
- [ ] HTTPS zorunlu kılın (reverse proxy veya IIS sertifikası)
- [ ] `SecurityHelper` içindeki AES anahtarını production için özelleştirin
- [ ] `AllowedHosts` değerini kendi domain'inize kısıtlayın
- [ ] Lisans anahtarını müşteri domain'ine göre üretin

## Sorun Giderme

**Migration hatası:** Connection string'i kontrol edin, veritabanı sunucusunun erişilebilir olduğundan emin olun.

**Admin panele erişilemiyor:** Production'da lisans anahtarının domain ile eşleştiğini doğrulayın.

**Seed verisi oluşmadı:** Kategori tablosu doluysa seed atlanır. Veritabanını sıfırlayıp yeniden başlatın.

**Tema görünmüyor:** `Views/Shared/Themes/{Tema}/_Layout.cshtml` dosyasının `@RenderBody()` içerdiğini kontrol edin.

## Destek

[Jetexsoft](https://jetexsoft.com.tr)
