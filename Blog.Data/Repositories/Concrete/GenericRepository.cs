using System.Linq.Expressions;
using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Data.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Repositories.Concrete;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public IQueryable<T> GetAll() => _dbSet.AsNoTracking();
    public IQueryable<T> Where(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate);
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Remove(T entity) => _dbSet.Remove(entity);
}