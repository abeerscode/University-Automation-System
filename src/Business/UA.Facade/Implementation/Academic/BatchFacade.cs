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
public sealed class BatchFacade(IBatchRepository batches, IRepository<Section> sections,
    IRepository<UA.Entities.Student> students, IDepartmentRepository departments, IRepository<AcademicProgram> programs,
    ICurriculumRepository versions, IUserAccessFacade access, ICurrentUser current,
    ISectionAllocator allocator, ICurriculumVersionResolver resolver, IUnitOfWork work,
    IConfigurationFacade config, IAcademicTermRepository terms) : IBatchFacade
{
    public async Task<IReadOnlyList<BatchDto>> ListAsync(int? departmentId = null, CancellationToken ct = default)
    {
        await access.RequireAsync("batches.manage", departmentId, ct);
        var all = await batches.ListAsync(x => !departmentId.HasValue || x.DepartmentId == departmentId.Value, ct);
        var ids = all.Select(x => x.BatchId).ToArray();
        var ss = await sections.ListAsync(x => ids.Contains(x.BatchId), ct);
        var sectionIds = ss.Select(x => x.SectionId).ToArray();
        var people = await students.ListAsync(x => sectionIds.Contains(x.SectionId), ct);
        var depts = await departments.ListAsync(ct: ct);
        var vs = await versions.ListAsync(ct: ct);
        var results = new List<BatchDto>();
        foreach (var batch in all.OrderByDescending(x => x.AdmissionYear))
        {
            if (current.UserId is not int uid || !await access.AllowedAsync(uid, "batches.manage", batch.DepartmentId, ct)) continue;
            var d = depts.First(x => x.DepartmentId == batch.DepartmentId);
            var v = vs.First(x => x.CurriculumId == batch.CurriculumId);
            results.Add(new(batch.BatchId, batch.DepartmentId, d.Code, batch.AdmissionYear, v.CurriculumId, v.VersionNo,
                batch.SeatLimit, ss.Where(s => s.BatchId == batch.BatchId).OrderBy(x => x.Name)
                    .Select(s => new SectionDto(s.SectionId, s.Name, s.Capacity,
                        people.Count(x => x.SectionId == s.SectionId))).ToArray()));
        }
        return results;
    }
    public async Task<int> CreateAsync(BatchInput input, CancellationToken ct = default)
    {
        await access.RequireAsync("batches.manage", input.DepartmentId, ct);
        var dept = await departments.FirstAsync(x => x.DepartmentId == input.DepartmentId && x.Status == "Active", ct)
            ?? throw new BusinessRuleException("Active department not found.");
        var program = await programs.FirstAsync(x => x.DepartmentId == dept.DepartmentId, ct)
            ?? throw new BusinessRuleException("Define this department's academic program first.");
        if (input.Year is < 2000 or > 2100 || input.Seats is < 1 or > 10000 || input.SectionCapacity is < 1 or > 1000)
            throw new BusinessRuleException("Invalid year, seats or section capacity.");
        var allVersions = await versions.ListAsync(x => x.ProgramId == program.ProgramId, ct);
        var linked = resolver.Resolve(program.ProgramId, input.Year,
            allVersions.Select(x => new CurriculumCandidate(x.CurriculumId, x.ProgramId, x.EffectiveFromYear, x.VersionNo, x.Status)));
        var term = (await terms.ListAsync(x => x.Year <= input.Year, ct))
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.TermNo).FirstOrDefault();
        // Configuration applies per department and academic term. Input values are
        // fallbacks only for installations that have not configured these keys yet.
        string? seatSetting = term is null ? null : await config.ResolveAsync("batch.seats", input.DepartmentId, term.TermId, ct);
        string? sectionSetting = term is null ? null : await config.ResolveAsync(UA.Framework.Constants.ConfigKeys.SectionCapacity, input.DepartmentId, term.TermId, ct);
        int seatCount = int.TryParse(seatSetting, out int parsedSeats) ? parsedSeats : input.Seats;
        int sectionCapacity = int.TryParse(sectionSetting, out int parsedSection) ? parsedSection : input.SectionCapacity;
        var plan = allocator.Plan(seatCount, sectionCapacity);
        Batch? created = null;
        await work.ExecuteTransactionAsync(async token =>
        {
            if (await batches.FirstAsync(x => x.DepartmentId == input.DepartmentId && x.AdmissionYear == input.Year, token) is not null)
                throw new BusinessRuleException("A batch already exists for this department and year.");
            created = new Batch { DepartmentId = input.DepartmentId, CurriculumId = linked,
                AdmissionYear = input.Year, SeatLimit = checked((short)seatCount) };
            await batches.AddAsync(created, token);
            // EF navigation propagates generated identity without manual SaveChanges.
            foreach (var section in plan) await sections.AddAsync(new Section { Batch = created, Name = section.Name, Capacity = section.Capacity }, token);
        }, ct);
        return created!.BatchId;
    }
    public async Task<int> AddSectionAsync(int batchId, string name, short capacity, CancellationToken ct = default)
    {
        var batch = await batches.FirstAsync(x => x.BatchId == batchId, ct) ?? throw new NotFoundException("Batch not found.");
        await access.RequireAsync("batches.manage", batch.DepartmentId, ct);
        name = name.Trim().ToUpperInvariant();
        if (name.Length is < 1 or > 10 || capacity is < 1 or > 1000)
            throw new BusinessRuleException("Section name or capacity invalid.");
        if (await sections.FirstAsync(x => x.BatchId == batchId && x.Name == name, ct) is not null)
            throw new BusinessRuleException("Section name already exists in this batch.");
        var section = new Section { BatchId = batchId, Name = name, Capacity = capacity };
        await sections.AddAsync(section, ct); await work.SaveChangesAsync(ct); return section.SectionId;
    }
}
