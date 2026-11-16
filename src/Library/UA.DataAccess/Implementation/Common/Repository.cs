using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Interface.Common;
namespace UA.DataAccess.Implementation.Common;
public class Repository<T>(UniversityDbContext db) : IRepository<T> where T : class
{
    public Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) => db.Set<T>().FirstOrDefaultAsync(predicate, ct);
    public Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => (predicate is null ? db.Set<T>().AsQueryable() : db.Set<T>().Where(predicate)).ToListAsync(ct);
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => (predicate is null ? db.Set<T>().AsQueryable() : db.Set<T>().Where(predicate)).CountAsync(ct);
    public Task AddAsync(T entity, CancellationToken ct = default) => db.Set<T>().AddAsync(entity, ct).AsTask();
    public void Remove(T entity) => db.Set<T>().Remove(entity);
}
