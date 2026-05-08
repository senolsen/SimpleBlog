namespace Blog.Core.Entities;

public class License : BaseEntity
{
    // Şifreli anahtar burada tutulacak
    public string Key { get; set; } = string.Empty;

    // Opsiyonel: Müşteri adı veya notu
    public string? CustomerName { get; set; }

    // Lisans aktif mi?
    public bool IsActive { get; set; } = true;
}