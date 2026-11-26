using UA.DataAccess.Interface.Faculty;
using UA.Facade.Interface.Administration;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Rules.Faculty;
/// <summary>Phase 4 reusable guard: every Phase 6/7 offering read or write must invoke this before accessing course data.</summary>
public interface ITeacherAssignmentGuard
{
    Task RequireAssignedAsync(int offeringId, CancellationToken ct = default);
}
public sealed class TeacherAssignmentGuard(IFacultyRepository faculty, ITeacherAssignmentRepository assignments,
    IUserAccessFacade access, ICurrentUser current, IClock clock) : ITeacherAssignmentGuard
{
    public async Task RequireAssignedAsync(int offeringId, CancellationToken ct = default)
    {
        if (offeringId <= 0 || current.UserId is not int userId || !await access.IsActiveAsync(userId, ct))
            throw new ForbiddenException("A current faculty assignment is required.");
        var roles = (await access.ForUserAsync(userId, ct)).Assignments;
        if (!roles.Any(a => a.RoleName is RoleNames.Faculty or RoleNames.GuestFaculty))
            throw new ForbiddenException("A faculty role is required.");
        var teacher = await faculty.ByAccountAsync(userId, ct);
        if (teacher is null || teacher.Status != "Active" ||
            !await assignments.IsAssignedAsync(teacher.FacultyId, offeringId, clock.Today, ct))
            throw new ForbiddenException("This course is not currently assigned to your faculty account.");
    }
}
