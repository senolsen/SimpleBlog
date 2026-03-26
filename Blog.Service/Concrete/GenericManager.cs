using System.Linq.Expressions;
using Blog.Core.Entities;
using Blog.Data.Context;
using Blog.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Blog.Service.Concrete;

public class GenericManager<T> : IGenericService<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericManager(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        // Sadece okuma işlemi yapılacağı için AsNoTracking() ile performansı artırıyoruz
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        entity.UpdatedDate = DateTime.Now; // BaseEntity'den gelen özellik otomatik güncelleniyor

        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(T entity)
    {
        // Veriyi fiziksel olarak silmek yerine (Soft Delete) pasife çekiyoruz
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.Now;

        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Eğer fiziksel olarak tamamen silmek istersen üsttekiler yerine şunu kullanabilirsin:
        // _dbSet.Remove(entity);
        // await _context.SaveChangesAsync();
    }
}