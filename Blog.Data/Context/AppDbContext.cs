using Blog.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Context;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<PostImage> PostImages { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }

    // YENİ EKLENEN TABLOLAR
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // PostTag (Çoka-Çok İlişki) tablosu için Composite Primary Key (Birleşik Anahtar) ayarı
        builder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId });

        // Post ile PostTag arasındaki ilişki
        builder.Entity<PostTag>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId);

        // Tag ile PostTag arasındaki ilişki
        builder.Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId);
    }
}