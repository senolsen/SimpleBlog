using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeneratorLicenceCode.Data;

public class LicenceDbContextFactory : IDesignTimeDbContextFactory<LicenceDbContext>
{
    public LicenceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LicenceDbContext>();
        optionsBuilder.UseSqlite("Data Source=licence.db");
        return new LicenceDbContext(optionsBuilder.Options);
    }
}
