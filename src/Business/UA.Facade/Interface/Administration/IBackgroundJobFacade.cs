using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IBackgroundJobFacade
{
    Task<IReadOnlyList<JobDto>> ListAsync(CancellationToken ct = default);
    Task<JobDto?> GetAsync(int id, CancellationToken ct = default);
    Task<int> EnqueueAsync(string jobType, CancellationToken ct = default);
}
