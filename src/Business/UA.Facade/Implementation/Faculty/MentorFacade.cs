using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Faculty;
using UA.Entities;
using UA.Facade.DTO.Faculty;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Faculty;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Faculty;
public sealed class MentorFacade(IMentorAssignmentRepository mentors, IFacultyRepository faculty,
    IRepository<Batch> batches, IRepository<Section> sections, IUnitOfWork work,
    IUserAccessFacade access, ICurrentUser current, IClock clock) : IMentorFacade
{
    private static MentorAssignmentDto Map(MentorAssignment a) => new(a.AssignmentId, a.FacultyId,
        a.Faculty.FullName, a.BatchId, a.Batch.AdmissionYear, a.SectionId, a.Section?.Name,
        a.AssignedOn, a.EndedOn);
    public async Task<IReadOnlyList<MentorAssignmentDto>> ListAsync(int departmentId, CancellationToken ct = default)
    {
        await access.RequireAsync("mentors.manage", departmentId, ct);
        return (await mentors.ForDepartmentAsync(departmentId, ct)).Select(Map).ToArray();
    }
    public async Task<MentorOptionsDto> OptionsAsync(int departmentId, CancellationToken ct = default)
    {
        await access.RequireAsync("mentors.manage", departmentId, ct);
        var allBatches = await batches.ListAsync(b => b.DepartmentId == departmentId, ct);
        var batchIds = allBatches.Select(b => b.BatchId).ToArray();
        var allSections = await sections.ListAsync(s => batchIds.Contains(s.BatchId), ct);
        // The faculty-to-mentor lookup is department-scoped. Cross-department teaching remains possible later.
        var (staff, _) = await faculty.SearchAsync(null, [departmentId], departmentId, null, "Active", 0, 500, ct);
        return new MentorOptionsDto(allBatches.OrderByDescending(b => b.AdmissionYear)
            .Select(b => new MentorBatchOptionDto(b.BatchId, b.AdmissionYear.ToString(),
                allSections.Where(s => s.BatchId == b.BatchId).OrderBy(s => s.Name)
                    .Select(s => new MentorSectionOptionDto(s.SectionId, s.Name)).ToArray())).ToArray(),
            staff.Where(f => f.UserId > 0).Select(f => new MentorFacultyOptionDto(f.FacultyId,
                f.FullName + " (" + f.EmployeeRef + ")")).OrderBy(f => f.Label).ToArray());
    }
    public async Task<int> AssignAsync(MentorSaveDto input, CancellationToken ct = default)
    {
        if (input.FacultyId <= 0 || input.BatchId <= 0) throw new BusinessRuleException("Select a valid faculty member and batch.");
        var batch = await batches.FirstAsync(x => x.BatchId == input.BatchId, ct)
            ?? throw new NotFoundException("Batch not found.");
        await access.RequireAsync("mentors.manage", batch.DepartmentId, ct);
        var teacher = await faculty.ByIdAsync(input.FacultyId, ct)
            ?? throw new NotFoundException("Faculty member not found.");
        if (teacher.Status != "Active" || teacher.User.Status != "Active")
            throw new BusinessRuleException("Only an active faculty member may become a mentor.");
        if (teacher.DepartmentId != batch.DepartmentId)
            throw new BusinessRuleException("A mentor must belong to the selected batch's department.");
        if (input.SectionId is int section && await sections.FirstAsync(s => s.SectionId == section &&
            s.BatchId == input.BatchId, ct) is null)
            throw new BusinessRuleException("Section must belong to the selected batch.");
        MentorAssignment? created = null;
        await work.ExecuteTransactionAsync(async token =>
        {
            var previous = await mentors.ActiveForTargetAsync(input.BatchId, input.SectionId, token);
            if (previous?.FacultyId == input.FacultyId)
                throw new BusinessRuleException("This mentor already holds the selected assignment.");
            if (previous is not null) previous.EndedOn = clock.Today;
            var next = new MentorAssignment { FacultyId = input.FacultyId, BatchId = input.BatchId,
                SectionId = input.SectionId, AssignedOn = clock.Today };
            await mentors.AddAsync(next, token);
            // IDs materialize after UnitOfWork commits; retrieved below using the same tracked entity.
            created = next;
        }, ct);
        return created?.AssignmentId ?? throw new InvalidOperationException("Mentor assignment was not saved.");
    }
    public async Task EndAsync(int id, CancellationToken ct = default)
    {
        await work.ExecuteTransactionAsync(async token =>
        {
            var x = await mentors.ByIdAsync(id, token) ?? throw new NotFoundException();
            await access.RequireAsync("mentors.manage", x.Batch.DepartmentId, token);
            if (x.EndedOn is not null) throw new BusinessRuleException("Assignment is already ended.");
            x.EndedOn = clock.Today;
        }, ct);
    }
    public async Task<IReadOnlyList<MenteeSummaryDto>> OwnMenteesAsync(CancellationToken ct = default)
    {
        int id = current.UserId ?? throw new ForbiddenException();
        if (!await access.IsActiveAsync(id, ct) || !(await access.ForUserAsync(id, ct)).Assignments.Any(a =>
            a.RoleName is RoleNames.Faculty or RoleNames.GuestFaculty)) throw new ForbiddenException();
        var teacher = await faculty.ByAccountAsync(id, ct);
        if (teacher is null || teacher.Status != "Active") throw new ForbiddenException();
        var assigned = await mentors.ForFacultyAsync(teacher.FacultyId, ct);
        var students = new Dictionary<string, MenteeSummaryDto>(StringComparer.Ordinal);
        foreach (var m in assigned.Where(a => a.EndedOn == null && a.AssignedOn.Date <= clock.Today))
            foreach (var s in await mentors.MenteesAsync(m.BatchId, m.SectionId, ct))
                students.TryAdd(s.StudentId, new MenteeSummaryDto(s.StudentId, s.FullName, s.RollNo,
                    s.Section.Name, s.Status));
        return students.Values.OrderBy(s => s.StudentId).ToArray();
    }
}
