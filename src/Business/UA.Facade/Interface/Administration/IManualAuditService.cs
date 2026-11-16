namespace UA.Facade.Interface.Administration;
public interface IManualAuditService
{
    Task LogAsync(string action, string entityName, string entityId, string? detail = null, CancellationToken ct = default);
}
