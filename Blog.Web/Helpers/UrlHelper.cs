using System.Text.RegularExpressions;
namespace Blog.Web.Helpers
{
    public static class UrlHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) return string.Empty;

            string str = phrase.ToLowerInvariant();

            // Türkçe karakterleri çevir
            str = str.Replace("ö", "o").Replace("ü", "u").Replace("ı", "i")
                     .Replace("ş", "s").Replace("ç", "c").Replace("ğ", "g");

            // Geçersiz karakterleri sil (sadece harf, rakam ve boşluk kalsın)
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Boşlukları tireye çevir
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 100 ? str.Length : 100).Trim(); // Maksimum 100 karakter
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }
    }
}
