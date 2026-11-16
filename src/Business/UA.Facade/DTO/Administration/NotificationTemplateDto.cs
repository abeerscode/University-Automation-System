namespace UA.Facade.DTO.Administration;
public sealed record NotificationTemplateDto(int Id, string Code, string Channel, string? Subject, string Body, bool Active);
