namespace Blog.Core.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Bir etiket birden fazla makalede kullanılabilir
    public ICollection<PostTag>? PostTags { get; set; }
}