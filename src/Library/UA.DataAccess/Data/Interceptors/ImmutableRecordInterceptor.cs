using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UA.Entities;
namespace UA.DataAccess.Data.Interceptors;
public sealed class ImmutableRecordInterceptor : SaveChangesInterceptor
{
    private static void Verify(DbContext? db)
    {
        if (db is null) return;
        foreach(var e in db.ChangeTracker.Entries().Where(e => (e.Entity is AuditEntry or Payment or PaymentAllocation or Receipt or Result) && e.State is EntityState.Modified or EntityState.Deleted))
        {
            // A-15 permits the ownership link only; amounts, status, allocations and receipts remain immutable.
            if(e.Entity is Payment p && e.State==EntityState.Modified && p.ApplicantId is int applicantId && p.StudentId is string studentId &&
               e.Property(nameof(Payment.StudentId)).OriginalValue is null && e.Properties.Where(x=>x.IsModified).All(x=>x.Metadata.Name==nameof(Payment.StudentId)) &&
               db.ChangeTracker.Entries<Student>().Any(s=>s.Entity.StudentId==studentId && s.Entity.ApplicationId is int applicationId &&
                 db.ChangeTracker.Entries<Application>().Any(a=>a.Entity.ApplicationId==applicationId && a.Entity.ApplicantId==applicantId && a.Entity.ApplicationStatus=="Enrolled"))) continue;
            throw new InvalidOperationException("Immutable financial, result or audit records cannot be edited/deleted.");
        }
    }
    public override InterceptionResult<int> SavingChanges(DbContextEventData data, InterceptionResult<int> result)
    { Verify(data.Context); return base.SavingChanges(data, result); }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    { Verify(data.Context); return base.SavingChangesAsync(data, result, cancellationToken); }
}
