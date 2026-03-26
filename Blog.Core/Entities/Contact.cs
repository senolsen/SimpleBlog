namespace Blog.Core.Entities;

// Projendeki temel sınıfın adının BaseEntity olduğunu varsayıyorum. 
// (Farklıysa ona göre değiştirebilirsin)
public class Contact : BaseEntity
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? MapIframeSrc { get; set; }
}