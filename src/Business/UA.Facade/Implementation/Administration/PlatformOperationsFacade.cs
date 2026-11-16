using Microsoft.Extensions.Logging;
using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class PlatformOperationsFacade(IRepository<Department> departments,
    INotificationRepository notifications, IRepository<NotificationTemplate> templates,
    IUserAccountRepository users, IUnitOfWork work, IEmailSender email,
    IClock clock, ILogger<PlatformOperationsFacade> logger) : IPlatformOperationsFacade
{
    public async Task<bool> CheckDatabaseAsync(CancellationToken ct = default)
    { try { await departments.CountAsync(ct: ct); return true; } catch (Exception ex) { logger.LogWarning(ex, "Database health probe failed"); return false; } }
    public async Task DispatchNotificationsAsync(CancellationToken ct = default)
    {
        var pending = (await notifications.ListAsync(x => x.Status == "Queued" && x.Attempts < 4, ct))
            .Where(n => n.QueuedAt.AddMinutes(n.Attempts switch { 1 => 5, 2 => 20, 3 => 80, _ => 0 }) <= clock.Now)
            .OrderBy(n => n.QueuedAt).Take(25).ToList();
        foreach (var item in pending)
        {
            ct.ThrowIfCancellationRequested();
            var template = await templates.FirstAsync(x => x.TemplateId == item.TemplateId && x.IsActive, ct);
            var user = await users.FirstAsync(x => x.UserId == item.RecipientUserId, ct);
            try
            {
                if (template is null || user is null) throw new InvalidOperationException("Invalid recipient or template.");
                if (template.Channel.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(user.Email)) throw new InvalidOperationException("Recipient email unavailable.");
                    await email.SendAsync(user.Email, template.Subject ?? "UAS notification", template.Body, ct);
                }
                else if (!template.Channel.Equals("In-app", StringComparison.OrdinalIgnoreCase))
                    throw new NotSupportedException("SMS transport not configured.");
                item.Status = "Sent"; item.SentAt = clock.Now;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Notification {NotificationId} delivery failed", item.NotificationId);
                item.Attempts++;
                if (item.Attempts >= 4) item.Status = "Failed";
            }
            await work.SaveChangesAsync(ct);
        }
    }
}
