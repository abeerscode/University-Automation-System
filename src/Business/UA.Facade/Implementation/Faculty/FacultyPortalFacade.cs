using UA.DataAccess.Interface.Faculty;
using UA.Facade.DTO.Faculty;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Faculty;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Faculty;
public sealed class FacultyPortalFacade(IFacultyRepository faculty, ITeacherAssignmentRepository assignments,
    IMentorAssignmentRepository mentors, IUserAccessFacade access, ICurrentUser current, IClock clock) : IFacultyPortalFacade
{
    public async Task<FacultyDashboardDto> DashboardAsync(CancellationToken ct = default)
    {
        var x = await OwnAsync(ct);
        var assigned = await CoursesAsync(ct: ct);
        var mentees = await mentors.ForFacultyAsync(x.FacultyId, ct);
        return new FacultyDashboardDto(new FacultyDetailDto(x.FacultyId, x.UserId,
            x.User.Username, x.EmployeeRef, x.FullName, x.DepartmentId, x.Department.Code,
            x.Designation, x.FacultyType, x.Status, x.Qualifications, x.Phone, x.User.Email,
            !string.IsNullOrEmpty(x.PhotoPath)), assigned,
            mentees.Count(a => a.EndedOn is null && a.AssignedOn.Date <= clock.Today));
    }
    public async Task<IReadOnlyList<TeachingAssignmentDto>> CoursesAsync(int? termId = null, CancellationToken ct = default)
    {
        var x = await OwnAsync(ct);
        var rows = await assignments.ForFacultyAsync(x.FacultyId, termId, ct);
        return rows.Where(a => a.AssignedOn.Date <= clock.Today &&
            (a.EndedOn is null || a.EndedOn.Value.Date > clock.Today) && a.Offering.Status != "Closed")
            .Select(TeachingAssignmentMapper.Map).ToArray();
    }
    public async Task<UA.Entities.Faculty> OwnAsync(CancellationToken ct = default)
    {
        int id = current.UserId ?? throw new ForbiddenException("Sign in with a faculty account.");
        if (!await access.IsActiveAsync(id, ct)) throw new ForbiddenException();
        var snapshot = await access.ForUserAsync(id, ct);
        if (!snapshot.Assignments.Any(a => a.RoleName is RoleNames.Faculty or RoleNames.GuestFaculty))
            throw new ForbiddenException("A faculty account is required.");
        var x = await faculty.ByAccountAsync(id, ct);
        if (x is null || x.Status != "Active") throw new ForbiddenException("Your faculty profile is not active.");
        // Db navigation User is loaded by ID via repository; do not expose EF entities to MVC.
        return await faculty.ByIdAsync(x.FacultyId, ct) ?? throw new NotFoundException();
    }
}
public sealed class TeacherAssignmentFacade(IFacultyPortalFacade portal) : ITeacherAssignmentFacade
{
    public Task<IReadOnlyList<TeachingAssignmentDto>> OwnAsync(int? termId = null, CancellationToken ct = default)
        => portal.CoursesAsync(termId, ct);
}
internal static class TeachingAssignmentMapper
{
    public static TeachingAssignmentDto Map(UA.Entities.OfferingTeacher a) =>
        new(a.OfferingId, a.Offering.TermId, $"{a.Offering.Term.Year} / Term {a.Offering.Term.TermNo}",
            a.Offering.Course.Code, a.Offering.Course.Title, a.Offering.Section.Name,
            a.Offering.Course.Credits, a.Offering.Status, a.AssignedOn, a.EndedOn);
}
