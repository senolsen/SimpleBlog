using System.Linq.Expressions;
using Blog.Core.Entities;

namespace Blog.Data.Repositories.Abstract;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    IQueryable<T> GetAll();
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}