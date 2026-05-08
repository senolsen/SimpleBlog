using Blog.Core.Helpers;

Console.WriteLine("-------------------------------------------");
Console.WriteLine("    Jetexsoft Blog Lisans Üretici v1.0");
Console.WriteLine("-------------------------------------------");

while (true)
{
    Console.Write("\nLisanslanacak Domain (örn: musteri.com): ");
    string? domain = Console.ReadLine();

    if (string.IsNullOrEmpty(domain)) break;

    try
    {
        // 1. Şifrele
        string encryptedKey = SecurityHelper.EncryptDomain(domain.Trim().ToLower());

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n [BAŞARILI] Üretilen Lisans Anahtarı:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine(encryptedKey);
        Console.WriteLine("------------------------------------------------------------");

        // 2. Test Et (Kendi kendini doğrula)
        string testDecrypt = SecurityHelper.DecryptDomain(encryptedKey);
        Console.WriteLine($"Test Doğrulaması: {testDecrypt} (Eşleşme: {domain.ToLower() == testDecrypt})");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Hata oluştu: " + ex.Message);
        Console.ResetColor();
    }

    Console.WriteLine("\nYeni bir anahtar üretmek için devam edin, çıkmak için Enter'a basın...");
}