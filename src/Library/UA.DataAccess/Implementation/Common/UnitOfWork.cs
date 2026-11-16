using UA.DataAccess.Data;
using UA.DataAccess.Interface.Common;
namespace UA.DataAccess.Implementation.Common;
public sealed class UnitOfWork(UniversityDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    public async Task ExecuteTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        await operation(ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
