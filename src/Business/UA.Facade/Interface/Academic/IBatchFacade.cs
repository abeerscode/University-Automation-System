using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface IBatchFacade
{
Task<IReadOnlyList<BatchDto>> ListAsync(int? departmentId = null, CancellationToken ct = default);
Task<int> CreateAsync(BatchInput input, CancellationToken ct = default);
Task<int> AddSectionAsync(int batchId, string name, short capacity, CancellationToken ct = default);
}
