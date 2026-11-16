using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Website;
public interface INoticeFacade
{
    Task<IReadOnlyList<NoticeDto>> ListAsync(int? departmentId = null, CancellationToken ct = default);
    Task<int> DraftAsync(int? departmentId, string title, string body, bool isPublic, CancellationToken ct = default);
    Task ApproveAsync(int noticeId, CancellationToken ct = default);
}
