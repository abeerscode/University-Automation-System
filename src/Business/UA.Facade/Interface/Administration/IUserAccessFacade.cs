using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IUserAccessFacade
{
    Task<AccessSnapshotDto> ForUserAsync(int userId, CancellationToken ct = default);
    Task<bool> AllowedAsync(int userId, string permission, int? departmentId = null, CancellationToken ct = default);
    Task RequireAsync(string permission, int? departmentId = null, CancellationToken ct = default);
    Task RequireGlobalAsync(string permission, CancellationToken ct = default);
    Task<bool> IsActiveAsync(int userId, CancellationToken ct = default);
}
