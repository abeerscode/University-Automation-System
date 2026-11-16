using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IUserFacade
{
    Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct = default);
    Task<UserDto?> GetAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(UserCreateDto input, CancellationToken ct = default);
    Task SetStatusAsync(int userId, bool active, CancellationToken ct = default);
    Task AssignRoleAsync(int userId, int roleId, int? departmentId, CancellationToken ct = default);
    Task RemoveRoleAsync(int userId, int roleId, CancellationToken ct = default);
}
