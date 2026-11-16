using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IConfigurationFacade
{
    Task<IReadOnlyList<ConfigDto>> HistoryAsync(string? key = null, CancellationToken ct = default);
    Task<string?> ResolveAsync(string key, int? departmentId, int termId, CancellationToken ct = default);
    Task<ConfigDto> SetAsync(string key, string value, int? departmentId, int termId, CancellationToken ct = default);
}
