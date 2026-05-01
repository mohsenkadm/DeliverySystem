using DeliverySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DeliverySystem.Infrastructure.Repositories;

/// <summary>المستودع العام الذي يُنفذ العمليات الأساسية على قاعدة البيانات</summary>
public class GenericRepository<T>(DbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    /// <summary>جلب كيان بواسطة المعرف</summary>
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    /// <summary>جلب جميع الكيانات</summary>
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    /// <summary>جلب الكيانات بشرط</summary>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    /// <summary>جلب أول كيان يحقق الشرط</summary>
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);

    /// <summary>إضافة كيان</summary>
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    /// <summary>إضافة مجموعة كيانات</summary>
    public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

    /// <summary>تحديث كيان</summary>
    public void Update(T entity) => _dbSet.Update(entity);

    /// <summary>حذف كيان</summary>
    public void Remove(T entity) => _dbSet.Remove(entity);

    /// <summary>حذف مجموعة كيانات</summary>
    public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    /// <summary>التحقق من وجود كيان</summary>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    /// <summary>عد الكيانات</summary>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    /// <summary>إرجاع IQueryable للاستعلامات المعقدة مع Include</summary>
    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
