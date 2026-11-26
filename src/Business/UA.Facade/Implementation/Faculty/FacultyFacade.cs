using System.Net.Mail;
using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Faculty;
using UA.DataAccess.Interface.Administration;
using UA.Entities;
using UA.Facade.DTO.Faculty;
using UA.Facade.Interface.Faculty;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Facade.Rules.Faculty;
using UA.Framework.Common;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Faculty;
public sealed class FacultyFacade(IFacultyRepository faculty, IUserAccountRepository users,
    IRepository<Department> departments, IRepository<UserRole> userRoles, IRepository<Role> roles,
    ITeacherAssignmentRepository teacherAssignments, IWorkloadCalculator workload,
    IUnitOfWork work, IUserAccessFacade access, ICurrentUser current, ISecureTokenGenerator random,
    IPasswordHasher passwords, IFacultyPhotoStorage storage, IFileStorage files, IClock clock) : IFacultyFacade
{
    private static FacultySummaryDto Summary(UA.Entities.Faculty x) =>
        new(x.FacultyId, x.EmployeeRef, x.FullName, x.DepartmentId, x.Department.Code,
            x.Designation, x.FacultyType, x.Status);
    private static FacultyDetailDto Detail(UA.Entities.Faculty x) =>
        new(x.FacultyId, x.UserId, x.User.Username, x.EmployeeRef, x.FullName, x.DepartmentId,
            x.Department.Code, x.Designation, x.FacultyType, x.Status, x.Qualifications,
            x.Phone, x.User.Email, !string.IsNullOrWhiteSpace(x.PhotoPath));
    public async Task<PagedResult<FacultySummaryDto>> SearchAsync(FacultySearchInput filter, CancellationToken ct = default)
    {
        await access.RequireAsync("faculty.view", ct: ct);
        int userId = current.UserId ?? throw new ForbiddenException();
        var allowed = new List<int>();
        foreach (var d in await departments.ListAsync(ct: ct))
            if (await access.AllowedAsync(userId, "faculty.view", d.DepartmentId, ct)) allowed.Add(d.DepartmentId);
        int page = Math.Clamp(filter.Page, 1, 100000), size = Math.Clamp(filter.PageSize, 1, 100);
        var (items, total) = await faculty.SearchAsync(filter.Query, allowed.ToArray(), filter.DepartmentId,
            filter.Type, filter.Status, (page - 1) * size, size, ct);
        return new PagedResult<FacultySummaryDto>(items.Select(Summary).ToArray(), total, page, size);
    }
    public async Task<FacultyDetailDto> GetAsync(int id, CancellationToken ct = default)
    {
        var x = await faculty.ByIdAsync(id, ct) ?? throw new NotFoundException("Faculty member not found.");
        await access.RequireAsync("faculty.view", x.DepartmentId, ct);
        return Detail(x);
    }
    public async Task<FacultyCreatedDto> CreateAsync(FacultySaveDto input, byte[]? photo, string? mime, CancellationToken ct = default)
    {
        Validate(input, photo, mime);
        await access.RequireAsync("faculty.manage", input.DepartmentId, ct);
        if (await departments.FirstAsync(x => x.DepartmentId == input.DepartmentId && x.Status == "Active", ct) is null)
            throw new BusinessRuleException("Choose an active department.");
        if (await faculty.EmployeeRefExistsAsync(input.EmployeeRef.Trim(), ct) ||
            await users.FirstAsync(x => x.Username == input.EmployeeRef.Trim(), ct) is not null)
            throw new BusinessRuleException("Employee reference is already in use as a username.");
        if (!string.IsNullOrWhiteSpace(input.Email) &&
            await users.FirstAsync(x => x.Email == input.Email.Trim(), ct) is not null)
            throw new BusinessRuleException("Email already belongs to an account.");
        var role = await RoleForAsync(input.Type, ct);
        var password = "Ua" + random.GenerateToken()[..22] + "8";
        var account = new UserAccount { Username = input.EmployeeRef.Trim(), Email = Normalize(input.Email),
            Mobile = Normalize(input.Phone), PasswordHash = passwords.Hash(password),
            IsVerified = false, Status = input.Status == "Active" ? "Active" : "Inactive" };
        var x = new UA.Entities.Faculty { User = account, DepartmentId = input.DepartmentId,
            EmployeeRef = input.EmployeeRef.Trim(), FullName = input.FullName.Trim(),
            Designation = input.Designation.Trim(), FacultyType = input.Type, Status = input.Status,
            Qualifications = Normalize(input.Qualifications), Phone = Normalize(input.Phone) };
        string? created = null;
        if (photo is not null) try { created = await storage.SaveFacultyAsync(x.EmployeeRef, photo, mime!, ct); x.PhotoPath = created; }
        catch (ArgumentException ex) { throw new BusinessRuleException(ex.Message); }
        try
        {
            await work.ExecuteTransactionAsync(async token =>
            {
                // Username/employee reference and email are protected by existing unique indexes.
                if (await users.FirstAsync(u => u.Username == x.EmployeeRef ||
                    (account.Email != null && u.Email == account.Email), token) is not null)
                    throw new BusinessRuleException("Username or email was registered by another user.");
                await users.AddAsync(account, token);
                await userRoles.AddAsync(new UserRole { User = account, RoleId = role.RoleId,
                    DepartmentId = x.DepartmentId }, token);
                await faculty.AddAsync(x, token);
            }, ct);
        }
        catch
        {
            if (created is not null) await storage.DeleteFacultyAsync(created, CancellationToken.None);
            throw;
        }
        return new FacultyCreatedDto(x.FacultyId, account.Username, password);
    }
    public async Task EditAsync(int id, FacultySaveDto input, byte[]? photo, string? mime, CancellationToken ct = default)
    {
        Validate(input, photo, mime);
        var x = await faculty.ByIdAsync(id, ct) ?? throw new NotFoundException("Faculty member not found.");
        await access.RequireAsync("faculty.manage", x.DepartmentId, ct);
        await access.RequireAsync("faculty.manage", input.DepartmentId, ct);
        if (!string.Equals(x.EmployeeRef, input.EmployeeRef.Trim(), StringComparison.Ordinal))
            throw new BusinessRuleException("The permanent employee reference cannot be changed.");
        if (await departments.FirstAsync(d => d.DepartmentId == input.DepartmentId && d.Status == "Active", ct) is null)
            throw new BusinessRuleException("Choose an active department.");
        var updatedEmail = Normalize(input.Email);
        if (updatedEmail is not null && await users.FirstAsync(u => u.Email == updatedEmail && u.UserId != x.UserId, ct) is not null)
            throw new BusinessRuleException("Email already belongs to another account.");
        var role = await RoleForAsync(input.Type, ct);
        string? created = null;
        if (photo is not null) try { created = await storage.SaveFacultyAsync(x.EmployeeRef, photo, mime!, ct); }
        catch (ArgumentException ex) { throw new BusinessRuleException(ex.Message); }
        var previousPhoto = x.PhotoPath;
        try
        {
            await work.ExecuteTransactionAsync(async token =>
            {
                var oldRoles = await userRoles.ListAsync(r => r.UserId == x.UserId, token);
                var teacherRoleIds = (await roles.ListAsync(r => r.Name == RoleNames.Faculty ||
                    r.Name == RoleNames.GuestFaculty, token)).Select(r => r.RoleId).ToHashSet();
                foreach (var assigned in oldRoles.Where(r => teacherRoleIds.Contains(r.RoleId) && r.RoleId != role.RoleId))
                    userRoles.Remove(assigned);
                // EF may reject a remove/re-add of the same composite key; when unchanged, retain and update scope.
                var existing = oldRoles.FirstOrDefault(r => r.RoleId == role.RoleId);
                if (existing is not null)
                {
                    // Remove only the other faculty type, leaving this tracked row in place.
                    existing.DepartmentId = input.DepartmentId;
                }
                else await userRoles.AddAsync(new UserRole { UserId = x.UserId, RoleId = role.RoleId,
                    DepartmentId = input.DepartmentId }, token);
                x.DepartmentId = input.DepartmentId; x.FullName = input.FullName.Trim();
                x.Designation = input.Designation.Trim(); x.FacultyType = input.Type;
                x.Status = input.Status; x.Qualifications = Normalize(input.Qualifications);
                x.Phone = Normalize(input.Phone); x.User.Email = updatedEmail;
                x.User.Mobile = x.Phone;
                x.User.Status = input.Status == "Active" ? "Active" : "Inactive";
                if (created is not null) x.PhotoPath = created;
            }, ct);
        }
        catch
        {
            if (created is not null) await storage.DeleteFacultyAsync(created, CancellationToken.None);
            throw;
        }
        if (created is not null && previousPhoto is not null && previousPhoto != created)
        {
            // The database update has committed; an old-file cleanup failure must not undo it.
            try { await storage.DeleteFacultyAsync(previousPhoto, CancellationToken.None); }
            catch (IOException) { /* Leave orphan cleanup to an operator. */ }
            catch (UnauthorizedAccessException) { /* Do not report a failed profile update after commit. */ }
        }
    }
    public async Task<FacultyPhotoDto> PhotoAsync(int id, CancellationToken ct = default)
    {
        var x = await faculty.ByIdAsync(id, ct) ?? throw new NotFoundException();
        if (current.UserId != x.UserId) await access.RequireAsync("faculty.view", x.DepartmentId, ct);
        else if (!await access.IsActiveAsync(x.UserId, ct)) throw new ForbiddenException();
        if (string.IsNullOrWhiteSpace(x.PhotoPath)) throw new NotFoundException("No faculty photo has been uploaded.");
        return new FacultyPhotoDto(await files.ReadAsync(x.PhotoPath, ct),
            x.PhotoPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg");
    }
    public async Task<IReadOnlyList<FacultyWorkloadRowDto>> WorkloadAsync(int departmentId, int? termId, CancellationToken ct = default)
    {
        await access.RequireAsync("workload.view", departmentId, ct);
        var rows = await teacherAssignments.ForDepartmentAsync(departmentId, termId, ct);
        return rows.Where(a => a.AssignedOn.Date <= a.Offering.Term.EndDate.Date &&
                (!a.EndedOn.HasValue || a.EndedOn.Value.Date >= a.Offering.Term.StartDate.Date))
            .GroupBy(a => new { a.FacultyId, a.Faculty.EmployeeRef, a.Faculty.FullName,
                a.Offering.TermId, a.Offering.Term.Year, a.Offering.Term.TermNo })
            .OrderBy(g => g.Key.FullName).ThenByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.TermNo)
            .Select(g => new FacultyWorkloadRowDto(g.Key.FacultyId, g.Key.EmployeeRef, g.Key.FullName,
                g.Key.TermId, $"{g.Key.Year} / Term {g.Key.TermNo}",
                workload.Calculate(g.Select(x => x.Offering.Course.Credits)), g.Count())).ToArray();
    }
    private async Task<Role> RoleForAsync(string type, CancellationToken ct)
    {
        string roleName = type == "Guest" ? RoleNames.GuestFaculty : RoleNames.Faculty;
        return await roles.FirstAsync(r => r.Name == roleName, ct)
            ?? throw new BusinessRuleException("Faculty roles have not been seeded. Run the existing bootstrap seed.");
    }
    private static string? Normalize(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    private static void Validate(FacultySaveDto v, byte[]? photo, string? mime)
    {
        if (v.DepartmentId <= 0 || string.IsNullOrWhiteSpace(v.EmployeeRef) || v.EmployeeRef.Trim().Length > 30 ||
            !v.EmployeeRef.Trim().All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ||
            string.IsNullOrWhiteSpace(v.FullName) || v.FullName.Trim().Length > 100 ||
            string.IsNullOrWhiteSpace(v.Designation) || v.Designation.Trim().Length > 50 ||
            v.Type is not ("Full-time" or "Part-time" or "Guest") ||
            v.Status is not ("Active" or "Inactive" or "Left") ||
            v.Qualifications?.Trim().Length > 255 || v.Phone?.Trim().Length > 20 ||
            v.Email?.Trim().Length > 100 || (Normalize(v.Email) is string email && !MailAddress.TryCreate(email, out _)) ||
            (photo is not null && (photo.Length is < 1 or > 1_048_576 || mime is not ("image/png" or "image/jpeg"))))
            throw new BusinessRuleException("Invalid faculty profile, employee reference, type, status, email or photo.");
    }
}
