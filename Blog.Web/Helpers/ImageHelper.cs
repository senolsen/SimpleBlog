using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Blog.Web.Helpers;

public static class ImageHelper
{
    // maxWidt=1200 : Blog siteleri için ideal makale kapağı genişliğidir.
    public static async Task<string> UploadAndConvertToWebpAsync(IFormFile file, string webRootPath, string folderName, int maxWidth = 1200)
    {
        if (file == null || file.Length == 0) return string.Empty;

        string uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        // Her resim için eşsiz bir isim ve .webp uzantısı atıyoruz
        string uniqueFileName = Guid.NewGuid().ToString() + ".webp";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = file.OpenReadStream())
        using (var image = await Image.LoadAsync(stream))
        {
            // Eğer resim 1200 pikselden büyükse, en/boy oranını koruyarak küçült
            if (image.Width > maxWidth)
            {
                image.Mutate(x => x.Resize(maxWidth, 0));
            }

            // Google'ın sevdiği WebP formatında %80 kalite ile kaydet (gözle görülür kalite kaybı olmaz, boyut efsane düşer)
            var encoder = new WebpEncoder { Quality = 80 };
            await image.SaveAsWebpAsync(filePath, encoder);
        }

        // Veritabanına kaydedilecek yolu döndür
        return $"/uploads/{folderName}/{uniqueFileName}";
    }
}