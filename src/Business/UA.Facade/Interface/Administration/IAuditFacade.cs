using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IAuditFacade
{
    Task<IReadOnlyList<AuditDto>> SearchAsync(string? entity = null, CancellationToken ct = default);
    Task<byte[]> ExportCsvAsync(string? entity = null, CancellationToken ct = default);
}
