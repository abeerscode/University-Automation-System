using UA.DataAccess.Interface.Academic;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Administration;
using UA.Facade.Rules.Academic;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Academic;
public sealed class CurriculumFacade(ICurriculumRepository curricula, IRepository<AcademicProgram> programs,
    ICourseRepository courses, IRepository<CurriculumCourse> lines,
    IRepository<ApprovalRecord> approvals, IBatchRepository batches, IDepartmentRepository departments,
    IUserAccessFacade access, ICurrentUser current, IUnitOfWork work, IClock clock) : ICurriculumFacade
{
    public async Task<IReadOnlyList<CurriculumDto>> ListAsync(int? departmentId = null, CancellationToken ct = default)
    {
        await access.RequireAsync("curriculum.manage", departmentId, ct);
        var ps = await programs.ListAsync(x => !departmentId.HasValue || x.DepartmentId == departmentId.Value, ct);
        var pids = ps.Select(x => x.ProgramId).ToArray();
        var all = await curricula.ListAsync(x => pids.Contains(x.ProgramId), ct);
        var ids = all.Select(x => x.CurriculumId).ToArray();
        var rows = await lines.ListAsync(x => ids.Contains(x.CurriculumId), ct);
        var courseIds = rows.Select(r => r.CourseId).Distinct().ToArray();
        var cs = await courses.ListAsync(x => courseIds.Contains(x.CourseId), ct);
        var output = new List<CurriculumDto>();
        foreach (var v in all.OrderByDescending(v => v.EffectiveFromYear).ThenByDescending(v => v.VersionNo))
        {
            var p = ps.First(x => x.ProgramId == v.ProgramId);
            if (current.UserId is not int uid || !await access.AllowedAsync(uid, "curriculum.manage", p.DepartmentId, ct)) continue;
            var items = (from row in rows where row.CurriculumId == v.CurriculumId join c in cs on row.CourseId equals c.CourseId
                orderby row.SemesterNo, c.Code select new CurriculumRowDto(c.CourseId, c.Code, c.Title, c.Credits, row.SemesterNo)).ToArray();
            output.Add(new(v.CurriculumId, v.ProgramId, p.Name, p.DepartmentId, v.VersionNo, v.EffectiveFromYear, v.Status, v.ApprovalId, items));
        }
        return output;
    }
    public async Task<int> CreateDraftAsync(CurriculumInput input, CancellationToken ct = default)
    {
        var program = await programs.FirstAsync(x => x.ProgramId == input.ProgramId, ct) ?? throw new NotFoundException("Program not found.");
        await access.RequireAsync("curriculum.manage", program.DepartmentId, ct);
        if (input.EffectiveYear < clock.Today.Year || input.EffectiveYear > clock.Today.Year + 20)
            throw new BusinessRuleException("Draft must start with the current or a future admission year.");
        var versions = await curricula.ListAsync(x => x.ProgramId == input.ProgramId, ct);
        if (versions.Any(x => x.EffectiveFromYear == input.EffectiveYear && x.Status != "Retired"))
            throw new BusinessRuleException("A curriculum version already exists for that effective year.");
        var v = new CurriculumVersion { ProgramId = input.ProgramId, EffectiveFromYear = input.EffectiveYear,
            VersionNo = versions.Select(x => x.VersionNo).DefaultIfEmpty(0).Max() + 1, Status = "Draft" };
        await curricula.AddAsync(v, ct); await work.SaveChangesAsync(ct); return v.CurriculumId;
    }
    private async Task<(CurriculumVersion Version, AcademicProgram Program)> DraftAsync(int id, CancellationToken ct)
    {
        var v = await curricula.FirstAsync(x => x.CurriculumId == id, ct) ?? throw new NotFoundException("Curriculum version not found.");
        var p = await programs.FirstAsync(x => x.ProgramId == v.ProgramId, ct) ?? throw new NotFoundException("Program not found.");
        await access.RequireAsync("curriculum.manage", p.DepartmentId, ct);
        if (v.Status != "Draft" || await batches.CountAsync(x => x.CurriculumId == id, ct) > 0)
            throw new BusinessRuleException("Approved or batch-linked curriculum versions are immutable.");
        return (v, p);
    }
    public async Task SetCourseAsync(int curriculumId, int courseId, byte semester, CancellationToken ct = default)
    {
        var (_, p) = await DraftAsync(curriculumId, ct);
        if (semester is < 1 or > 8) throw new BusinessRuleException("Semester must be between 1 and 8.");
        var course = await courses.FirstAsync(x => x.CourseId == courseId && x.DepartmentId == p.DepartmentId, ct);
        if (course is null) throw new BusinessRuleException("Course must belong to this program's department.");
        var existing = await lines.FirstAsync(x => x.CurriculumId == curriculumId && x.CourseId == courseId, ct);
        if (existing is null) await lines.AddAsync(new CurriculumCourse { CurriculumId = curriculumId, CourseId = courseId, SemesterNo = semester }, ct);
        else existing.SemesterNo = semester;
        await work.SaveChangesAsync(ct);
    }
    public async Task RemoveCourseAsync(int curriculumId, int courseId, CancellationToken ct = default)
    {
        await DraftAsync(curriculumId, ct);
        var existing = await lines.FirstAsync(x => x.CurriculumId == curriculumId && x.CourseId == courseId, ct)
            ?? throw new NotFoundException("Curriculum course not found.");
        lines.Remove(existing); await work.SaveChangesAsync(ct);
    }
    public async Task ApproveAsync(int curriculumId, int approvalId, CancellationToken ct = default)
    {
        var (v, p) = await DraftAsync(curriculumId, ct);
        var dept = await departments.FirstAsync(x => x.DepartmentId == p.DepartmentId, ct) ?? throw new NotFoundException();
        var approval = await approvals.FirstAsync(x => x.ApprovalId == approvalId, ct);
        if (approval is null || approval.Status != "Approved" ||
            !approval.ApprovalType.Contains("Curriculum", StringComparison.OrdinalIgnoreCase) ||
            !approval.ReferenceNo.StartsWith(dept.Code + "-", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Provide an approved Curriculum approval record for this department.");
        var all = await lines.ListAsync(x => x.CurriculumId == curriculumId, ct);
        var courseIds = all.Select(x => x.CourseId).ToArray();
        var creditRows = await courses.ListAsync(x => courseIds.Contains(x.CourseId), ct);
        if (all.Count == 0 || all.Any(x => x.SemesterNo is < 1 or > 8) ||
            Enumerable.Range(1, 8).Any(sem =>
            { var credits = (from line in all where line.SemesterNo == sem join c in creditRows on line.CourseId equals c.CourseId select c.Credits).Sum();
                return credits < 12 || credits > 18; }) || creditRows.Sum(c => c.Credits) != p.TotalCredits)
            throw new BusinessRuleException("Before approval, supply all 8 semesters (12–18 credits each) and match program total credits.");
        var currentBatches = await batches.ListAsync(x => x.DepartmentId == p.DepartmentId && x.AdmissionYear >= v.EffectiveFromYear, ct);
        if (currentBatches.Count > 0)
            throw new BusinessRuleException("Existing batches cannot be retroactively assigned a new curriculum. Select a later effective year.");
        await work.ExecuteTransactionAsync(async token =>
        {
            if (await curricula.CountAsync(x => x.ApprovalId == approvalId && x.CurriculumId != curriculumId, token) > 0)
                throw new BusinessRuleException("Approval reference is already linked to another curriculum version.");
            if (await batches.CountAsync(x => x.DepartmentId == p.DepartmentId && x.AdmissionYear >= v.EffectiveFromYear, token) > 0)
                throw new BusinessRuleException("A batch was created for this period; select a later effective year.");
            var other = await curricula.ListAsync(x => x.ProgramId == p.ProgramId && x.Status == "Active", token);
            foreach (var prior in other) prior.Status = "Retired";
            v.Status = "Active"; v.ApprovalId = approvalId;
        }, ct);
    }
}
