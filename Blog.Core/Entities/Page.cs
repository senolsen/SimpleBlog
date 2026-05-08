namespace Blog.Core.Entities;

public class Page : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // domain.com/sayfa/hakkimizda şeklinde erişim için
    public bool IsActive { get; set; } = true;
}