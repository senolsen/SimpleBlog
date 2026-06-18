namespace Blog.Web.Services;

public interface IDataSeeder
{
    Task EnsureInfrastructureAsync();
    Task SeedDemoContentAsync();
    Task ResetToDemoAsync();
}
