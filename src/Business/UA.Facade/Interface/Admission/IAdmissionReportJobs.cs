using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Admission;
public interface IAdmissionReportJobs
{
 Task<int> EnqueueAsync(int circularId,int? departmentId,string? status,bool pdf);
 Task<JobDto> GetAsync(int id);
 Task<(byte[] Data,bool Pdf)> DownloadAsync(int id);
 Task<int?> NextAsync(CancellationToken ct);
 Task ProcessAsync(int id,CancellationToken ct);
 Task FailAsync(int id,CancellationToken ct);
 Task CleanupAsync(CancellationToken ct);
}
