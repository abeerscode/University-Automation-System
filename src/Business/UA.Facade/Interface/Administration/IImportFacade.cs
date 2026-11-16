using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IImportFacade
{
    Task<ImportValidationDto> ValidateCsvAsync(Stream input, IReadOnlyList<string> requiredHeaders, CancellationToken ct = default);
}
