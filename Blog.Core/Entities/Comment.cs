namespace Blog.Core.Entities;

public class Comment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Yorum varsayılan olarak onay bekler (Admin onaylamadan sitede gözükmez)
    public bool IsApproved { get; set; } = false;

    // İlişkiler (Her yorum bir makaleye aittir)
    public int PostId { get; set; }
    public Post? Post { get; set; }
}