using Blog.Core.Enums;

namespace Blog.Core.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImagePath { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<PostImage>? Images { get; set; }

    public string? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // --- YENİ EKLENEN SİSTEMLER ---

    // 1. Okunma Sayacı (View Count)
    public int ViewCount { get; set; } = 0;

    // 2. Yorumlar (Bir makalenin birden çok yorumu olabilir)
    public ICollection<Comment>? Comments { get; set; }

    // 3. Etiketler (Çoka-Çok ilişki bağlantısı)
    public ICollection<PostTag>? PostTags { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;
}