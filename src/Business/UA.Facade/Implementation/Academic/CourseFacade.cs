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
public sealed class CourseFacade(ICourseRepository courses, IDepartmentRepository departments,
    IRepository<CoursePrerequisite> prerequisites, IRepository<CurriculumCourse> curriculumCourses,
    IRepository<CurriculumVersion> versions, IUserAccessFacade access, ICurrentUser current,
    IUnitOfWork work) : ICourseFacade
{
    private static readonly string[] Types = ["Theory", "Lab", "Project", "Thesis", "Internship"];
    public async Task<IReadOnlyList<CourseDto>> ListAsync(int? departmentId = null, CancellationToken ct = default)
    {
        await access.RequireAsync("courses.manage", departmentId, ct);
        var all = await courses.ListAsync(x => !departmentId.HasValue || x.DepartmentId == departmentId.Value, ct);
        var ids = all.Select(x => x.CourseId).ToArray();
        var links = await prerequisites.ListAsync(x => ids.Contains(x.CourseId), ct);
        var output = new List<CourseDto>();
        foreach (var c in all.OrderBy(x => x.Code))
            if (current.UserId is int uid && await access.AllowedAsync(uid, "courses.manage", c.DepartmentId, ct))
                output.Add(Map(c, links));
        return output;
    }
    private static CourseDto Map(Course c, IEnumerable<CoursePrerequisite> links) => new(c.CourseId, c.DepartmentId,
        c.Code, c.Title, c.Credits, c.CourseType, links.Where(x => x.CourseId == c.CourseId).Select(x => x.PrerequisiteCourseId).ToArray());
    public async Task<CourseDto> SaveAsync(CourseInput input, CancellationToken ct = default)
    {
        await access.RequireAsync("courses.manage", input.DepartmentId, ct);
        var code = input.Code.Trim().ToUpperInvariant(); var title = input.Title.Trim();
        if (code.Length is < 2 or > 15 || title.Length is < 3 or > 150 ||
            input.Credits <= 0 || input.Credits > 18 || decimal.Round(input.Credits, 1) != input.Credits || !Types.Contains(input.Type))
            throw new BusinessRuleException("Enter a valid course code, title, type and credits (one decimal maximum).");
        if (await departments.FirstAsync(x => x.DepartmentId == input.DepartmentId && x.Status == "Active", ct) is null)
            throw new BusinessRuleException("Active department is required.");
        if (await courses.FirstAsync(x => x.Code == code && x.CourseId != input.Id, ct) is not null)
            throw new BusinessRuleException("Course code already exists.");
        var unique = input.Prerequisites.Distinct().ToArray();
        if (unique.Length != input.Prerequisites.Count || unique.Length > 25 || unique.Contains(input.Id) && input.Id != 0)
            throw new BusinessRuleException("Prerequisites must be unique and cannot reference the course itself.");
        var others = await courses.ListAsync(x => unique.Contains(x.CourseId), ct);
        if (others.Count != unique.Length || others.Any(x => x.DepartmentId != input.DepartmentId))
            throw new BusinessRuleException("Prerequisites must be existing courses from the same department.");
        var old = input.Id == 0 ? null : await courses.FirstAsync(x => x.CourseId == input.Id, ct) ?? throw new NotFoundException("Course not found.");
        if (old is not null && old.DepartmentId != input.DepartmentId) throw new BusinessRuleException("Course department cannot be changed.");
        var existingLinks = old is null ? new List<CoursePrerequisite>() : await prerequisites.ListAsync(x => x.CourseId == old.CourseId, ct);
        if (old is not null && (old.Code != code || old.Title != title || old.Credits != input.Credits || old.CourseType != input.Type ||
            !existingLinks.Select(x => x.PrerequisiteCourseId).Order().SequenceEqual(unique.Order())) &&
            await IsUsedByApprovedVersionAsync(old.CourseId, ct))
            throw new BusinessRuleException("An approved curriculum uses this course. Preserve it and create a new course for future versions.");
        if (old is not null && await WouldCycleAsync(old.CourseId, unique, ct))
            throw new BusinessRuleException("Prerequisites would create a circular dependency.");
        Course item = old ?? new Course { DepartmentId = input.DepartmentId };
        await work.ExecuteTransactionAsync(async token =>
        {
            item.Code = code; item.Title = title; item.Credits = input.Credits; item.CourseType = input.Type;
            if (old is null) { await courses.AddAsync(item, token); await work.SaveChangesAsync(token); }
            var linksToRemove = existingLinks.Where(x => !unique.Contains(x.PrerequisiteCourseId)).ToArray();
            foreach (var link in linksToRemove) prerequisites.Remove(link);
            foreach (var id in unique.Where(id => existingLinks.All(x => x.PrerequisiteCourseId != id)))
                await prerequisites.AddAsync(new CoursePrerequisite { CourseId = item.CourseId, PrerequisiteCourseId = id }, token);
        }, ct);
        return Map(item, await prerequisites.ListAsync(x => x.CourseId == item.CourseId, ct));
    }
    private async Task<bool> IsUsedByApprovedVersionAsync(int courseId, CancellationToken ct)
    {
        var use = await curriculumCourses.ListAsync(x => x.CourseId == courseId, ct);
        var ids = use.Select(x => x.CurriculumId).ToArray();
        return await versions.CountAsync(x => ids.Contains(x.CurriculumId) && x.Status != "Draft", ct) > 0;
    }
    private async Task<bool> WouldCycleAsync(int id, int[] proposed, CancellationToken ct)
    {
        var graph = (await prerequisites.ListAsync(ct: ct)).GroupBy(x => x.CourseId).ToDictionary(g => g.Key, g => g.Select(x => x.PrerequisiteCourseId).ToArray());
        graph[id] = proposed;
        var seen = new HashSet<int>(); var stack = new HashSet<int>();
        bool Visit(int node)
        {
            if (stack.Contains(node)) return true;
            if (!seen.Add(node)) return false;
            stack.Add(node);
            if (graph.TryGetValue(node, out var links)) foreach (var next in links) if (Visit(next)) return true;
            stack.Remove(node); return false;
        }
        return Visit(id);
    }
}
