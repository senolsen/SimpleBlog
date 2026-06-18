using GeneratorLicenceCode.Entities;
using Microsoft.EntityFrameworkCore;

namespace GeneratorLicenceCode.Data;

public class LicenceDbContext : DbContext
{
    public LicenceDbContext(DbContextOptions<LicenceDbContext> options) : base(options)
    {
    }

    public DbSet<LicenseRecord> Licenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LicenseRecord>(entity =>
        {
            entity.HasIndex(e => e.Domain);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.LicenseKey).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
