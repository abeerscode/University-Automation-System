using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
namespace UA.Facade.Implementation.Administration;
public sealed class RoleFacade(IRoleRepository roles, IRepository<RolePermission> grants, IUnitOfWork work, IUserAccessFacade access) : IRoleFacade
{
    public async Task<IReadOnlyList<RoleDto>> ListAsync(CancellationToken ct = default)
    { await access.RequireAsync("users.view", ct: ct); return (await roles.ListAsync(ct: ct)).Select(r => new RoleDto(r.RoleId, r.Name, r.DepartmentScoped)).ToList(); }
    public async Task<IReadOnlyList<string>> PermissionsAsync(int roleId, CancellationToken ct = default)
    { await access.RequireAsync("roles.manage", ct: ct); return (await grants.ListAsync(x => x.RoleId == roleId, ct)).Select(x => x.PermissionCode).ToList(); }
    public async Task<int> CreateAsync(string name, bool scoped, CancellationToken ct = default)
    {
        await access.RequireAsync("roles.manage", ct: ct);
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50) throw new BusinessRuleException("Invalid role name.");
        if (await roles.FirstAsync(x => x.Name == name, ct) is not null) throw new BusinessRuleException("Role exists.");
        var role = new Role { Name = name.Trim(), DepartmentScoped = scoped };
        await roles.AddAsync(role, ct); await work.SaveChangesAsync(ct); return role.RoleId;
    }
    public async Task SetPermissionsAsync(int roleId, IReadOnlyList<string> permissions, CancellationToken ct = default)
    {
        await access.RequireAsync("roles.manage", ct: ct);
        if (await roles.FirstAsync(x => x.RoleId == roleId, ct) is null) throw new NotFoundException();
        var newSet = permissions.Distinct().ToHashSet(StringComparer.Ordinal);
        var role = await roles.FirstAsync(x => x.RoleId == roleId, ct);
        if (role?.Name == RoleNames.SystemAdministrator &&
            (!newSet.Contains("roles.manage") || !newSet.Contains("users.manage") || !newSet.Contains("audit.view")))
            throw new BusinessRuleException("Core administrator privileges cannot be removed.");
        if (newSet.Any(code => code.Length > 50 || !PermissionCodes.All.Contains(code)))
            throw new BusinessRuleException("Unrecognized permission code.");
        var existing = await grants.ListAsync(x => x.RoleId == roleId, ct);
        foreach (var x in existing.Where(x => !newSet.Contains(x.PermissionCode))) grants.Remove(x);
        foreach (var code in newSet.Where(x => existing.All(e => e.PermissionCode != x)))
            await grants.AddAsync(new RolePermission { RoleId = roleId, PermissionCode = code }, ct);
        await work.SaveChangesAsync(ct);
    }
}
