namespace Blog.Core.Entities;

// BaseEntity'den miras ALMIYORUZ çünkü bu sadece bir bağlayıcı (Join) tablosudur.
public class PostTag
{
    public int PostId { get; set; }
    public Post? Post { get; set; }

    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}