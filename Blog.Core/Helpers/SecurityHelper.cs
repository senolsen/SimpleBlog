using System.Security.Cryptography;
using System.Text;

namespace Blog.Core.Helpers;

public static class SecurityHelper
{
    // BU ANAHTARI ASLA PAYLAŞMA VE DEĞİŞTİRME! (32 Karakterli olmalı)
    private static readonly string InternalKey = "7e1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o";
    private static readonly string InternalIV = "1a2b3c4d5e6f7g8h"; // 16 Karakterli

    public static string EncryptDomain(string domain)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(InternalKey);
        aes.IV = Encoding.UTF8.GetBytes(InternalIV);

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(domain.ToLower());
            }
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string DecryptDomain(string cipherText)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(InternalKey);
            aes.IV = Encoding.UTF8.GetBytes(InternalIV);

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }
}