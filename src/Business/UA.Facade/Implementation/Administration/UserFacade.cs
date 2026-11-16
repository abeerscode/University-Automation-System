using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Facade.Rules.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
namespace UA.Facade.Implementation.Administration;
public sealed class UserFacade(IUserAccountRepository users, IRoleRepository roles, IRepository<UserRole> assignments,
    IRepository<Department> departments, IUnitOfWork work, IPasswordHasher hasher, IPasswordPolicy passwordPolicy,
    IUserAccessFacade access, ICurrentUser current) : IUserFacade
{
    private static UserDto Map(UserAccount x) => new(x.UserId, x.Username, x.Email, x.Mobile, x.Status, x.IsVerified);
    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct = default)
    { await access.RequireAsync("users.view", ct: ct); return (await users.ListAsync(ct: ct)).OrderBy(x => x.Username).Take(500).Select(Map).ToList(); }
    public async Task<UserDto?> GetAsync(int id, CancellationToken ct = default)
    { await access.RequireAsync("users.view", ct: ct); var x = await users.FirstAsync(u => u.UserId == id, ct); return x is null ? null : Map(x); }
    public async Task<int> CreateAsync(UserCreateDto input, CancellationToken ct = default)
    {
        await access.RequireAsync("users.manage", ct: ct);
        if (string.IsNullOrWhiteSpace(input.Username) || input.Username.Length > 50) throw new BusinessRuleException("Invalid username.");
        var issue = passwordPolicy.Validate(input.Password); if (issue is not null) throw new BusinessRuleException(issue);
        if (await users.FirstAsync(x => x.Username == input.Username, ct) is not null) throw new BusinessRuleException("Username already exists.");
        var role = await roles.FirstAsync(x => x.RoleId == input.RoleId, ct) ?? throw new NotFoundException("Role not found.");
        if (role.DepartmentScoped && input.DepartmentId is null) throw new BusinessRuleException("A department is required for this role.");
        if (input.DepartmentId is int dep && await departments.FirstAsync(d => d.DepartmentId == dep, ct) is null)
            throw new BusinessRuleException("Department not found.");
        var x = new UserAccount { Username = input.Username.Trim(), PasswordHash = hasher.Hash(input.Password),
            Email = input.Email, Mobile = input.Mobile, Status = "Active", IsVerified = false };
        await work.ExecuteTransactionAsync(async token =>
        {
            await users.AddAsync(x, token); await work.SaveChangesAsync(token);
            await assignments.AddAsync(new UserRole { UserId = x.UserId, RoleId = input.RoleId,
                DepartmentId = role.DepartmentScoped ? input.DepartmentId : null }, token);
        }, ct);
        return x.UserId;
    }
    public async Task SetStatusAsync(int userId, bool active, CancellationToken ct = default)
    {
        await access.RequireAsync("users.manage", ct: ct);
        if (!active && current.UserId == userId) throw new BusinessRuleException("You cannot deactivate your own account.");
        var x = await users.FirstAsync(a => a.UserId == userId, ct) ?? throw new NotFoundException();
        x.Status = active ? "Active" : "Inactive"; await work.SaveChangesAsync(ct);
    }
    public async Task AssignRoleAsync(int userId, int roleId, int? departmentId, CancellationToken ct = default)
    {
        await access.RequireAsync("users.manage", ct: ct);
        if (await users.FirstAsync(u => u.UserId == userId, ct) is null) throw new NotFoundException();
        var role = await roles.FirstAsync(r => r.RoleId == roleId, ct) ?? throw new NotFoundException("Role not found.");
        if (role.DepartmentScoped && departmentId is null) throw new BusinessRuleException("Department required.");
        if (departmentId is int dep && await departments.FirstAsync(d => d.DepartmentId == dep, ct) is null)
            throw new BusinessRuleException("Department not found.");
        if (await assignments.FirstAsync(x => x.UserId == userId && x.RoleId == roleId, ct) is not null)
            throw new BusinessRuleException("This role is already assigned. The schema only allows one department per role.");
        await assignments.AddAsync(new UserRole { UserId = userId, RoleId = roleId, DepartmentId = role.DepartmentScoped ? departmentId : null }, ct);
        await work.SaveChangesAsync(ct);
    }
    public async Task RemoveRoleAsync(int userId, int roleId, CancellationToken ct = default)
    {
        await access.RequireAsync("users.manage", ct: ct);
        if (userId == current.UserId) throw new BusinessRuleException("Do not remove your own role.");
        var x = await assignments.FirstAsync(a => a.UserId == userId && a.RoleId == roleId, ct);
        if (x is not null) { assignments.Remove(x); await work.SaveChangesAsync(ct); }
    }
}
