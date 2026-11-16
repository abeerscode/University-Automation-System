using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IApprovalFacade
{
    Task<IReadOnlyList<ApprovalDto>> ListAsync(CancellationToken ct = default);
    Task<ApprovalDto> RecordAsync(string type, string status, int? departmentId, int requestorId, string? scanPath, CancellationToken ct = default);
    Task<ApprovalDto> CorrectAsync(int approvalId, string status, string? scanPath, CancellationToken ct = default);
}
