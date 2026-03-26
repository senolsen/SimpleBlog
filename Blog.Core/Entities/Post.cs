namespace Blog.Core.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; } // Ana kapak fotoğrafı yine kalsın

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Bir Post'un birden fazla resmi olabilir (Navigation Property)
    public ICollection<PostImage>? Images { get; set; }

    public string? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}