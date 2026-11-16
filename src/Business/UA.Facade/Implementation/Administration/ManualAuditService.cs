using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.Interface.Administration;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class ManualAuditService(IAuditRepository audit, IUnitOfWork work,
    ICurrentUser current, IClock clock) : IManualAuditService
{
    public async Task LogAsync(string action, string entityName, string entityId, string? detail = null, CancellationToken ct = default)
    {
        await audit.AddAsync(new AuditEntry { UserId = current.UserId, Action = action[..Math.Min(action.Length,50)],
            EntityName = entityName[..Math.Min(entityName.Length,50)], EntityId = entityId[..Math.Min(entityId.Length,30)],
            NewValue = detail, LoggedAt = clock.Now }, ct);
        await work.SaveChangesAsync(ct);
    }
}
