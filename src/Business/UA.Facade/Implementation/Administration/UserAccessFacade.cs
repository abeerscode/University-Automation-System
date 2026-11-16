using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Administration;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
namespace UA.Facade.Implementation.Administration;
public sealed class UserAccessFacade(
    IUserAccountRepository users, IRepository<UserRole> userRoles, IRoleRepository roles,
    IRepository<RolePermission> grants, ICurrentUser current) : IUserAccessFacade
{
    public async Task<bool> IsActiveAsync(int userId, CancellationToken ct = default)
    {
        var user = await users.FirstAsync(u => u.UserId == userId, ct);
        return user is { Status: "Active" };
    }
    public async Task<AccessSnapshotDto> ForUserAsync(int userId, CancellationToken ct = default)
    {
        if (!await IsActiveAsync(userId, ct)) return new([], []);
        var assigned = await userRoles.ListAsync(x => x.UserId == userId, ct);
        var roleIds = assigned.Select(x => x.RoleId).ToArray();
        var activeRoles = await roles.ListAsync(x => roleIds.Contains(x.RoleId), ct);
        var grantsList = await grants.ListAsync(x => roleIds.Contains(x.RoleId), ct);
        var assignments = (from a in assigned join r in activeRoles on a.RoleId equals r.RoleId
            select new RoleAssignmentDto(userId, r.RoleId, r.Name, a.DepartmentId, r.DepartmentScoped)).ToArray();
        return new(assignments, grantsList.Select(x => x.PermissionCode).Distinct().ToArray());
    }
    public async Task<bool> AllowedAsync(int userId, string permission, int? departmentId = null, CancellationToken ct = default)
    {
        if (!await IsActiveAsync(userId, ct)) return false;
        var assigned = await userRoles.ListAsync(x => x.UserId == userId, ct);
        if (assigned.Count == 0) return false;
        var roleIds = assigned.Select(x => x.RoleId).ToArray();
        var matching = await grants.ListAsync(x => x.PermissionCode == permission && roleIds.Contains(x.RoleId), ct);
        if (matching.Count == 0) return false;
        if (departmentId is null) return true; // screen-level check; data filtering happens in each facade
        var matchIds = matching.Select(x => x.RoleId).ToArray();
        var roleFlags = await roles.ListAsync(x => matchIds.Contains(x.RoleId), ct);
        return assigned.Any(a => matchIds.Contains(a.RoleId) && roleFlags.Any(r => r.RoleId == a.RoleId &&
            (!r.DepartmentScoped || a.DepartmentId == departmentId)));
    }
    public async Task RequireGlobalAsync(string permission, CancellationToken ct = default)
    {
        if (current.UserId is not int id) throw new ForbiddenException();
        var user = await ForUserAsync(id, ct);
        var grantsMatching = await grants.ListAsync(g => g.PermissionCode == permission, ct);
        var roleIds = grantsMatching.Select(g => g.RoleId).ToHashSet();
        if (!user.Assignments.Any(a => !a.Scoped && roleIds.Contains(a.RoleId)))
            throw new ForbiddenException("Global privileges are required for university-wide changes.");
    }
    public async Task RequireAsync(string permission, int? departmentId = null, CancellationToken ct = default)
    {
        if (current.UserId is not int id || !await AllowedAsync(id, permission, departmentId, ct))
            throw new ForbiddenException("You do not have permission for this action or department.");
    }
}
