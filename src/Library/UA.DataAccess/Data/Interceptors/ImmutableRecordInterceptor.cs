using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UA.Entities;
namespace UA.DataAccess.Data.Interceptors;
public sealed class ImmutableRecordInterceptor : SaveChangesInterceptor
{
    private static void Verify(DbContext? db)
    {
        if (db is null) return;
        if (db.ChangeTracker.Entries().Any(e =>
            (e.Entity is AuditEntry or Payment or Result) && e.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Immutable financial, result or audit records cannot be edited/deleted.");
    }
    public override InterceptionResult<int> SavingChanges(DbContextEventData data, InterceptionResult<int> result)
    { Verify(data.Context); return base.SavingChanges(data, result); }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    { Verify(data.Context); return base.SavingChangesAsync(data, result, cancellationToken); }
}
