using UA.DataAccess.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Framework.Security;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
namespace UA.Facade.Implementation.Administration;
public sealed class AuditFacade(IAuditRepository audit, IUserAccessFacade access, IExcelWriter writer, IManualAuditService manualAudit, ICurrentUser current) : IAuditFacade
{
    public async Task<byte[]> ExportCsvAsync(string? entity = null, CancellationToken ct = default)
    {
        await access.RequireAsync("exports.sensitive", ct: ct);
        var rows = await SearchAsync(entity, ct);
        var bytes = writer.WriteCsv(["ID", "User", "Action", "Entity", "EntityID", "DhakaTime", "Old", "New"],
            rows.Select(x => (IReadOnlyList<string>)[x.Id.ToString(), x.UserId?.ToString() ?? "", x.Action,
                x.Entity, x.Key, x.Timestamp.ToString("O"), x.Old ?? "", x.New ?? ""]));
        await manualAudit.LogAsync("Export", "AuditEntry", "CSV", $"Exported {rows.Count} audit records", ct);
        return bytes;
    }
    public async Task<IReadOnlyList<AuditDto>> SearchAsync(string? entity = null, CancellationToken ct = default)
    {
        await access.RequireAsync("audit.view", ct: ct);
        if (current.UserId is not int id || !(await access.ForUserAsync(id, ct)).Assignments.Any(a =>
            a.RoleName is RoleNames.SystemAdministrator or RoleNames.Registrar))
            throw new ForbiddenException("Audit is restricted to Administrators and the Registrar.");
        var items = string.IsNullOrWhiteSpace(entity) ? await audit.ListAsync(ct: ct)
            : await audit.ListAsync(x => x.EntityName == entity, ct);
        return items.OrderByDescending(x => x.AuditId).Take(500)
            .Select(x => new AuditDto(x.AuditId, x.UserId, x.Action, x.EntityName, x.EntityId,
                x.LoggedAt, x.OldValue, x.NewValue)).ToArray();
    }
}
