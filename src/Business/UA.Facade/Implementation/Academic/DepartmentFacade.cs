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
public sealed class DepartmentFacade(IDepartmentRepository departments, IRepository<AcademicProgram> programs,
    IAcademicTermRepository terms, IConfigurationFacade config, IUserAccessFacade access,
    ICurrentUser current, IUnitOfWork work, IRepository<CurriculumVersion> curriculumRecords) : IDepartmentFacade
{
    public Task<IReadOnlyList<DepartmentDto>> ListAsync(CancellationToken ct = default)
        => AvailableAsync("departments.manage", ct);
    public async Task<IReadOnlyList<DepartmentDto>> AvailableAsync(string permission, CancellationToken ct = default)
    {
        await access.RequireAsync(permission, ct: ct);
        var all = await departments.ListAsync(ct: ct);
        var linked = await programs.ListAsync(ct: ct);
        var output = new List<DepartmentDto>();
        foreach (var d in all.OrderBy(x => x.DepartmentId))
            if (current.UserId is int uid && await access.AllowedAsync(uid, permission, d.DepartmentId, ct))
            {
                var p = linked.FirstOrDefault(x => x.DepartmentId == d.DepartmentId);
                output.Add(new(d.DepartmentId, d.Code, d.Name, d.IdCode, d.Status, p?.ProgramId, p?.Name, p?.TotalCredits));
            }
        return output;
    }
    public async Task<DepartmentDto> UpdateAsync(int id, string name, string status, CancellationToken ct = default)
    {
        await access.RequireAsync("departments.manage", id, ct);
        var dept = await departments.FirstAsync(x => x.DepartmentId == id, ct) ?? throw new NotFoundException("Department not found.");
        name = name.Trim();
        if (name.Length is < 3 or > 100 || status is not ("Active" or "Inactive")) throw new BusinessRuleException("Invalid department name or status.");
        if (await departments.FirstAsync(x => x.DepartmentId != id && x.Name == name, ct) is not null)
            throw new BusinessRuleException("Department name already exists.");
        dept.Name = name; dept.Status = status;
        await work.SaveChangesAsync(ct);
        var p = await programs.FirstAsync(x => x.DepartmentId == id, ct);
        return new(dept.DepartmentId, dept.Code, dept.Name, dept.IdCode, dept.Status, p?.ProgramId, p?.Name, p?.TotalCredits);
    }
    public async Task UpsertProgramAsync(ProgramInput input, CancellationToken ct = default)
    {
        await access.RequireAsync("departments.manage", input.DepartmentId, ct);
        if (await departments.FirstAsync(x => x.DepartmentId == input.DepartmentId, ct) is null) throw new NotFoundException("Department not found.");
        var name = input.Name.Trim();
        if (name.Length is < 3 or > 100 || input.TotalCredits is < 96 or > 144)
            throw new BusinessRuleException("Program name or total credits are invalid (8 semesters at 12–18 credits).");
        var existing = await programs.FirstAsync(x => x.DepartmentId == input.DepartmentId, ct);
        if (existing is null)
            await programs.AddAsync(new AcademicProgram { DepartmentId = input.DepartmentId, Name = name,
                TotalCredits = input.TotalCredits, SemesterCount = 8 }, ct);
        else
        {
            if (existing.TotalCredits != input.TotalCredits &&
                await accessToCurricula(existing.ProgramId, ct))
                throw new BusinessRuleException("Program total credits cannot change after a curriculum has been defined.");
            existing.Name = name; existing.TotalCredits = input.TotalCredits;
        }
        await work.SaveChangesAsync(ct);
    }
    // Queries remain in the facade's data layer abstraction rather than in a controller.
    private async Task<bool> accessToCurricula(int programId, CancellationToken ct)
        => await curricula.CountAsync(x => x.ProgramId == programId, ct) != 0;
    // Initialized via primary constructor without exposing persistence entities to UI.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044")]
    private readonly IRepository<CurriculumVersion> curricula = curriculumRecords;
    public async Task<DepartmentSettingsDto> SettingsAsync(int departmentId, int termId, CancellationToken ct = default)
    {
        await access.RequireAsync("departments.manage", departmentId, ct);
        if (await departments.FirstAsync(x => x.DepartmentId == departmentId, ct) is null) throw new NotFoundException("Department not found.");
        if (await terms.FirstAsync(x => x.TermId == termId, ct) is null) throw new NotFoundException("Academic term not found.");
        string? credits = await config.ResolveAsync(UA.Framework.Constants.ConfigKeys.NormalCredits, departmentId, termId, ct);
        string? attendance = await config.ResolveAsync(UA.Framework.Constants.ConfigKeys.AttendanceMinimum, departmentId, termId, ct);
        string? starts = await config.ResolveAsync("class.start_time", departmentId, termId, ct);
        string? ends = await config.ResolveAsync("class.end_time", departmentId, termId, ct);
        string? seats = await config.ResolveAsync("batch.seats", departmentId, termId, ct);
        string? capacity = await config.ResolveAsync(UA.Framework.Constants.ConfigKeys.SectionCapacity, departmentId, termId, ct);
        return new(departmentId, termId, int.TryParse(credits, out var c) ? c : 15,
            decimal.TryParse(attendance, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var a) ? a : 75,
            starts ?? "09:00", ends ?? "17:00",
            int.TryParse(seats, out var b) ? b : 120, int.TryParse(capacity, out var sc) ? sc : 40);
    }
    public async Task SaveSettingsAsync(DepartmentSettingsInput input, CancellationToken ct = default)
    {
        await access.RequireAsync("departments.manage", input.DepartmentId, ct);
        await access.RequireAsync("config.manage", input.DepartmentId, ct);
        if (input.NormalCredits is < 12 or > 18 || input.MinimumAttendance is < 75 or > 100 ||
            input.SeatsPerIntake is < 1 or > 10000 || input.SectionCapacity is < 1 or > 1000)
            throw new BusinessRuleException("Normal credits must be 12–18 and attendance must be at least 75%.");
        if (!TimeOnly.TryParseExact(input.ClassStarts, "HH:mm", out var start) ||
            !TimeOnly.TryParseExact(input.ClassEnds, "HH:mm", out var end) || start >= end)
            throw new BusinessRuleException("Class start and end must use HH:mm; start must be before end.");
        await config.SetAsync(UA.Framework.Constants.ConfigKeys.NormalCredits, input.NormalCredits.ToString(), input.DepartmentId, input.TermId, ct);
        await config.SetAsync(UA.Framework.Constants.ConfigKeys.AttendanceMinimum,
            input.MinimumAttendance.ToString(System.Globalization.CultureInfo.InvariantCulture), input.DepartmentId, input.TermId, ct);
        await config.SetAsync("class.start_time", input.ClassStarts, input.DepartmentId, input.TermId, ct);
        await config.SetAsync("class.end_time", input.ClassEnds, input.DepartmentId, input.TermId, ct);
        await config.SetAsync("batch.seats", input.SeatsPerIntake.ToString(), input.DepartmentId, input.TermId, ct);
        await config.SetAsync(UA.Framework.Constants.ConfigKeys.SectionCapacity, input.SectionCapacity.ToString(), input.DepartmentId, input.TermId, ct);
    }
}
