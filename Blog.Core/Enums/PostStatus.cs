namespace Blog.Core.Enums;

public enum PostStatus
{
    Draft = 0,      // Taslak (Sadece admin/yazar görür)
    Published = 1,  // Yayında (Herkes görür)
    Deleted = 2     // Çöp Kutusu (Opsiyonel: Fiziksel silmek yerine buraya atılabilir)
}