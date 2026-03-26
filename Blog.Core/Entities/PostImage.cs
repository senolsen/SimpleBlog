namespace Blog.Core.Entities;

public class PostImage : BaseEntity
{
    public string ImagePath { get; set; } = string.Empty;

    // SEO ve erişilebilirlik (Accessibility) için Alt metin çok önemlidir
    public string? AltText { get; set; }

    // Hangi Post'a ait olduğunu belirten Foreign Key
    public int PostId { get; set; }
    public Post? Post { get; set; }
}