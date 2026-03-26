using Microsoft.AspNetCore.Identity;

namespace Blog.Core.Entities;

public class AppRole : IdentityRole
{
    // IdentityRole içinde zaten Id ve Name var. 
    // Biz ekstra olarak rolün ne işe yaradığını anlatan bir alan ekliyoruz.
    public string? Description { get; set; }
}