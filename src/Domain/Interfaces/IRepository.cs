using System.Linq.Expressions;

namespace DeliverySystem.Domain.Interfaces;

/// <summary>واجهة المستودع العامة للعمليات الأساسية على قاعدة البيانات</summary>
public interface IRepository<T> where T : class
{
    /// <summary>جلب كيان بواسطة المعرف</summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>جلب جميع الكيانات</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>جلب الكيانات بشرط معين</summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>جلب كيان واحد بشرط معين</summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>إضافة كيان جديد</summary>
    Task AddAsync(T entity);

    /// <summary>إضافة مجموعة كيانات</summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>تحديث كيان</summary>
    void Update(T entity);

    /// <summary>حذف كيان</summary>
    void Remove(T entity);

    /// <summary>حذف مجموعة كيانات</summary>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>التحقق من وجود كيان بشرط معين</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>عدد الكيانات بشرط معين</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>جلب كيانات مع تضمين بيانات مرتبطة</summary>
    IQueryable<T> Query();
}
