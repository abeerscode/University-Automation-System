using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class NotificationFacade(INotificationRepository notifications, IRepository<NotificationTemplate> templates,
    IUnitOfWork work, ICurrentUser current, IClock clock, IUserAccessFacade access) : INotificationFacade
{
    public async Task<IReadOnlyList<NotificationTemplateDto>> TemplatesAsync(CancellationToken ct = default)
    {
        await access.RequireAsync("templates.manage", ct: ct);
        return (await templates.ListAsync(ct: ct)).Select(x => new NotificationTemplateDto(x.TemplateId, x.Code,
            x.Channel, x.Subject, x.Body, x.IsActive)).ToArray();
    }
    public async Task SetTemplateAsync(int templateId, string? subject, string body, bool enabled, CancellationToken ct = default)
    {
        await access.RequireAsync("templates.manage", ct: ct);
        if (subject?.Length > 150) throw new BusinessRuleException("Subject is too long.");
        var t = await templates.FirstAsync(x => x.TemplateId == templateId, ct) ?? throw new NotFoundException();
        t.Subject = subject; t.Body = body; t.IsActive = enabled;
        await work.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<NotificationDto>> InboxAsync(CancellationToken ct = default)
    {
        var id = current.UserId ?? throw new ForbiddenException();
        var items = await notifications.ListAsync(n => n.RecipientUserId == id, ct);
        var templateIds = items.Select(x => x.TemplateId).Distinct().ToArray();
        var rows = await templates.ListAsync(t => templateIds.Contains(t.TemplateId), ct);
        return (from n in items join t in rows on n.TemplateId equals t.TemplateId
            orderby n.QueuedAt descending select new NotificationDto(n.NotificationId, t.Subject ?? "Notification", t.Body, n.Status, n.QueuedAt)).Take(100).ToList();
    }
    public async Task MarkReadAsync(long id, CancellationToken ct = default)
    {
        if (current.UserId is not int userId) throw new ForbiddenException();
        var n = await notifications.FirstAsync(n => n.NotificationId == id && n.RecipientUserId == userId, ct) ?? throw new NotFoundException();
        if (n.Status is "Sent" or "Queued") n.Status = "Read";
        await work.SaveChangesAsync(ct);
    }
    public async Task QueueAsync(int userId, string templateCode, string eventKey, CancellationToken ct = default)
    {
        var template = await templates.FirstAsync(t => t.Code == templateCode && t.IsActive, ct) ?? throw new NotFoundException("Template not found.");
        if (eventKey.Length > 100 || eventKey.Length == 0) throw new BusinessRuleException("Invalid event key.");
        if (await notifications.FirstAsync(n => n.EventKey == eventKey && n.RecipientUserId == userId, ct) is not null) return;
        await notifications.AddAsync(new Notification { TemplateId = template.TemplateId, RecipientUserId = userId,
            EventKey = eventKey, Status = "Queued", QueuedAt = clock.Now }, ct);
        await work.SaveChangesAsync(ct);
    }
}
