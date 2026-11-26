using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Website;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Website;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Website;

public sealed class NoticeFacade(INoticeRepository notices, IUnitOfWork work, IUserAccessFacade access,
    ICurrentUser current, IClock clock) : INoticeFacade
{
    private static NoticeDto Map(Notice n) => new(n.NoticeId, n.DepartmentId, n.Title, n.Body, n.IsPublic, n.PublishedAt, n.AuthorId, n.ApprovedBy);
    public async Task<IReadOnlyList<NoticeDto>> ListAsync(int? departmentId = null, CancellationToken ct = default)
    {
        var list = await notices.ListAsync(n => (departmentId == null || n.DepartmentId == departmentId || n.DepartmentId == null), ct);
        var result = new List<NoticeDto>();
        foreach (var n in list.OrderByDescending(n => n.NoticeId))
        {
            var allowedToEdit = current.UserId is int id && await access.AllowedAsync(id, "notices.edit", n.DepartmentId, ct);
            IReadOnlyList<RoleAssignmentDto> assignments = current.UserId is int userId ? (await access.ForUserAsync(userId, ct)).Assignments : Array.Empty<RoleAssignmentDto>();
            var inScope = n.DepartmentId is null || assignments.Any(a => !a.Scoped || a.DepartmentId == n.DepartmentId);
            if ((n.PublishedAt is not null && (n.IsPublic || current.IsAuthenticated && inScope)) || allowedToEdit)
                result.Add(Map(n));
        }
        return result.Take(200).ToArray();
    }
    public async Task<int> DraftAsync(int? departmentId, string title, string body, bool isPublic, CancellationToken ct = default)
    {
        if (departmentId is null) await access.RequireGlobalAsync("notices.edit", ct);
        else await access.RequireAsync("notices.edit", departmentId, ct);
        if (current.UserId is not int id) throw new ForbiddenException();
        if (title.Length is < 1 or > 200) throw new BusinessRuleException("Title must be 1–200 characters.");
        var notice = new Notice
        {
            DepartmentId = departmentId,
            Title = title,
            Body = body,
            IsPublic = isPublic,
            AuthorId = id
        };
        await notices.AddAsync(notice, ct); await work.SaveChangesAsync(ct); return notice.NoticeId;
    }
    public async Task ApproveAsync(int noticeId, CancellationToken ct = default)
    {
        var notice = await notices.FirstAsync(n => n.NoticeId == noticeId, ct) ?? throw new NotFoundException();
        if (notice.DepartmentId is null) await access.RequireGlobalAsync("notices.approve", ct);
        else await access.RequireAsync("notices.approve", notice.DepartmentId, ct);
        if (notice.AuthorId == current.UserId) throw new BusinessRuleException("Author cannot approve their own notice.");
        if (notice.PublishedAt is not null) throw new BusinessRuleException("Notice already approved.");
        notice.ApprovedBy = current.UserId; notice.PublishedAt = clock.Now;
        await work.SaveChangesAsync(ct);
    }
}
