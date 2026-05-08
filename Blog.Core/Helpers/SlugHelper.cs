using System.Text.RegularExpressions;

namespace Blog.Core.Helpers;

public static class SlugHelper
{
    public static string MakeSlug(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // 1. Önce tüm harfleri küçült (Türkçe karakter sorununu aşmak için ToLowerInvariant)
        string str = text.ToLowerInvariant();

        // 2. Türkçe karakterleri İngilizce karşılıklarına çevir
        str = str.Replace("ö", "o")
                 .Replace("ü", "u")
                 .Replace("ş", "s")
                 .Replace("ı", "i")
                 .Replace("ğ", "g")
                 .Replace("ç", "c");

        // 3. Harf, rakam, boşluk ve tire DIŞINDAKİ tüm özel karakterleri sil
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        // 4. Yan yana gelmiş birden fazla boşluğu tek boşluğa düşür ve kenarlardaki boşlukları kırp
        str = Regex.Replace(str, @"\s+", " ").Trim();

        // 5. Boşlukları tire (-) işareti ile değiştir
        str = Regex.Replace(str, @"\s", "-");

        // 6. Yan yana gelmiş birden fazla tireyi tek tireye düşür (Örn: yazi---baslik -> yazi-baslik)
        str = Regex.Replace(str, @"-+", "-");

        return str;
    }
}