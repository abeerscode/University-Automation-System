using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface INotificationFacade
{
    Task<IReadOnlyList<NotificationDto>> InboxAsync(CancellationToken ct = default);
    Task<IReadOnlyList<NotificationTemplateDto>> TemplatesAsync(CancellationToken ct = default);
    Task SetTemplateAsync(int templateId, string? subject, string body, bool enabled, CancellationToken ct = default);
    Task MarkReadAsync(long id, CancellationToken ct = default);
    Task QueueAsync(int userId, string templateCode, string eventKey, CancellationToken ct = default);
}
