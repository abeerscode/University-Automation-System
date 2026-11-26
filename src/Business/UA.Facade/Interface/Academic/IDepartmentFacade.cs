using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface IDepartmentFacade
{
Task<IReadOnlyList<DepartmentDto>> ListAsync(CancellationToken ct = default);
Task<IReadOnlyList<DepartmentDto>> AvailableAsync(string permission, CancellationToken ct = default);
Task<DepartmentDto> UpdateAsync(int id, string name, string status, CancellationToken ct = default);
Task UpsertProgramAsync(ProgramInput input, CancellationToken ct = default);
Task<DepartmentSettingsDto> SettingsAsync(int departmentId, int termId, CancellationToken ct = default);
Task SaveSettingsAsync(DepartmentSettingsInput input, CancellationToken ct = default);
}
