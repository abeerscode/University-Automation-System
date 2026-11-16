using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class AcademicRulesFacade(IRepository<GradeScale> grades, IRepository<MarkComponent> marks,
    IRepository<AttendanceScale> attendance, IRepository<Department> departments,
    IUserAccessFacade access, IUnitOfWork work, IClock clock) : IAcademicRulesFacade
{
    private async Task ValidateDepartment(int id, CancellationToken ct)
    {
        await access.RequireAsync("config.manage", id, ct);
        if (await departments.FirstAsync(x => x.DepartmentId == id, ct) is null) throw new NotFoundException("Department not found.");
    }
    private void ValidateDate(DateTime date)
    { if (date.Date < clock.Today) throw new BusinessRuleException("New rule versions cannot take effect in the past."); }
    public async Task<IReadOnlyList<GradeBandDto>> GradeBandsAsync(int deptId, CancellationToken ct = default)
    {
        await ValidateDepartment(deptId, ct);
        return (await grades.ListAsync(x => x.DepartmentId == deptId, ct)).OrderByDescending(x => x.VersionNo)
            .ThenByDescending(x => x.MinMark).Select(x => new GradeBandDto(x.LetterGrade, x.MinMark, x.MaxMark, x.GradePoint, x.VersionNo)).ToArray();
    }
    public async Task<IReadOnlyList<MarkPartDto>> MarkPartsAsync(int deptId, string courseType, CancellationToken ct = default)
    {
        await ValidateDepartment(deptId, ct);
        return (await marks.ListAsync(x => x.DepartmentId == deptId && x.CourseType == courseType, ct))
            .OrderByDescending(x => x.VersionNo).Select(x => new MarkPartDto(x.CourseType, x.Name, x.MaxMarks, x.IsAttendanceBased, x.VersionNo)).ToArray();
    }
    public async Task<IReadOnlyList<AttendanceBandDto>> AttendanceBandsAsync(int deptId, CancellationToken ct = default)
    {
        await ValidateDepartment(deptId, ct);
        return (await attendance.ListAsync(x => x.DepartmentId == deptId, ct))
            .OrderByDescending(x => x.VersionNo).ThenByDescending(x => x.MinPercent)
            .Select(x => new AttendanceBandDto(x.MinPercent, x.MarkPercent, x.VersionNo)).ToArray();
    }
    public async Task<int> AddGradeVersionAsync(int deptId, DateTime effectiveFrom, IReadOnlyList<GradeBandDto> bands, CancellationToken ct = default)
    {
        await ValidateDepartment(deptId, ct); ValidateDate(effectiveFrom);
        var ordered = bands.OrderBy(x => x.MinMark).ToList();
        if (ordered.Count != 10 || ordered[0].MinMark != 0 || ordered[^1].MaxMark != 100 ||
            ordered.Any(x => x.MinMark < 0 || x.MaxMark > 100 || x.MinMark >= x.MaxMark || x.GradePoint is < 0 or > 4) ||
            ordered.Select(x => x.LetterGrade).Distinct().Count() != 10 ||
            Enumerable.Range(1, ordered.Count-1).Any(i => ordered[i-1].MaxMark != ordered[i].MinMark))
            throw new BusinessRuleException("Grade bands must contain 10 non-overlapping intervals covering 0 through 100.");
        var old = await grades.ListAsync(x => x.DepartmentId == deptId, ct);
        int version = old.Select(x => x.VersionNo).DefaultIfEmpty(0).Max() + 1;
        foreach (var b in ordered) await grades.AddAsync(new GradeScale { DepartmentId = deptId, VersionNo = version,
            LetterGrade = b.LetterGrade, MinMark = b.MinMark, MaxMark = b.MaxMark,
            GradePoint = b.GradePoint, EffectiveFrom = effectiveFrom.Date }, ct);
        await work.SaveChangesAsync(ct); return version;
    }
    public async Task<int> AddMarkSchemeVersionAsync(int deptId, DateTime effectiveFrom, string courseType,
        IReadOnlyList<MarkPartDto> parts, CancellationToken ct = default)
    {
        await ValidateDepartment(deptId, ct); ValidateDate(effectiveFrom);
        if (courseType.Length is < 1 or > 15 || parts.Count is < 1 or > 15 || parts.Sum(x => x.MaxMarks) != 100 ||
            parts.Any(x => x.MaxMarks <= 0 || x.Name.Length is < 1 or > 40) ||
            parts.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != parts.Count)
            throw new BusinessRuleException("Component names must be unique and marks must total exactly 100.");
        var old = await marks.ListAsync(x => x.DepartmentId == deptId && x.CourseType == courseType, ct);
        int version = old.Select(x => x.VersionNo).DefaultIfEmpty(0).Max() + 1;
        foreach (var p in parts) await marks.AddAsync(new MarkComponent { DepartmentId = deptId,
            CourseType = courseType, Name = p.Name, MaxMarks = p.MaxMarks, VersionNo = version,
            IsAttendanceBased = p.AttendanceBased, EffectiveFrom = effectiveFrom.Date }, ct);
        await work.SaveChangesAsync(ct); return version;
    }
    public async Task<int> AddAttendanceVersionAsync(int deptId, DateTime effectiveFrom, IReadOnlyList<AttendanceBandDto> bands, CancellationToken ct = default)
    {
        await ValidateDepartment(deptId, ct); ValidateDate(effectiveFrom);
        var ordered = bands.OrderBy(x => x.MinPercent).ToList();
        if (ordered.Count < 2 || ordered[0].MinPercent != 0 ||
            ordered.Select(x => x.MinPercent).Distinct().Count() != ordered.Count ||
            ordered.Any(x => x.MinPercent is < 0 or > 100 || x.MarkPercent is < 0 or > 100) ||
            Enumerable.Range(1, ordered.Count-1).Any(i => ordered[i].MarkPercent < ordered[i-1].MarkPercent))
            throw new BusinessRuleException("Attendance bands must start at 0, with unique thresholds and non-decreasing marks.");
        var old = await attendance.ListAsync(x => x.DepartmentId == deptId, ct);
        int version = old.Select(x => x.VersionNo).DefaultIfEmpty(0).Max() + 1;
        foreach (var b in ordered) await attendance.AddAsync(new AttendanceScale { DepartmentId = deptId,
            VersionNo = version, MinPercent = b.MinPercent, MarkPercent = b.MarkPercent, EffectiveFrom = effectiveFrom.Date }, ct);
        await work.SaveChangesAsync(ct); return version;
    }
}
