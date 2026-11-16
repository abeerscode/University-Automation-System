namespace UA.Facade.Interface.Administration;
public interface IPlatformOperationsFacade
{
    Task<bool> CheckDatabaseAsync(CancellationToken ct = default);
    Task DispatchNotificationsAsync(CancellationToken ct = default);
}
