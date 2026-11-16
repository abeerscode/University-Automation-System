using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IRoleFacade
{
    Task<IReadOnlyList<RoleDto>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> PermissionsAsync(int roleId, CancellationToken ct = default);
    Task<int> CreateAsync(string name, bool scoped, CancellationToken ct = default);
    Task SetPermissionsAsync(int roleId, IReadOnlyList<string> permissions, CancellationToken ct = default);
}
