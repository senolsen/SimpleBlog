namespace Blog.Data.UnitOfWorks;

public interface IUnitOfWork : IAsyncDisposable
{
    Task CommitAsync();
    void Commit();
}