using Blog.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Context;

// Sırasıyla: Kullanıcı Sınıfı, Rol Sınıfı, Primary Key Tipi (Id'ler default string'dir)
public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<PostImage> PostImages { get; set; } // Eklediysen

    public DbSet<Contact> Contacts { get; set; }
}