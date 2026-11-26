using UA.Facade.DTO.Student;
using UA.Framework.Common;
namespace UA.Facade.Interface.Student;
public interface IStudentFacade
{
    Task<PagedResult<StudentListItem>> SearchAsync(StudentSearchInput input, CancellationToken ct = default);
    Task<StudentDetailDto> GetAsync(string id, CancellationToken ct = default);
    Task UpdateAsync(StudentEditInput input, CancellationToken ct = default);
}
public interface IStudentStatusFacade
{
    Task ChangeAsync(StudentStatusChangeInput input, CancellationToken ct = default);
}
public interface ISectionChangeFacade
{
    Task ChangeAsync(SectionChangeInput input, CancellationToken ct = default);
}
public interface IStudentPortalFacade
{
    Task<StudentDetailDto> OwnProfileAsync(CancellationToken ct = default);
    Task UpdateOwnAsync(string phone, string address, byte[]? photo, string? contentType, CancellationToken ct = default);
    Task<StudentPhotoDto> OwnPhotoAsync(CancellationToken ct = default);
}
public interface IStudentImportFacade
{
    Task<StudentImportPreview> PreviewAsync(byte[] data, string filename, CancellationToken ct = default);
    Task<StudentImportResult> CommitAsync(byte[] data, string filename, CancellationToken ct = default);
}
/// <summary>Enrollment (Phase 10) and student import (Phase 3) use the same account/record creation service.
/// Caller is responsible for wrapping creation in a transaction and saving changes.</summary>
public interface IStudentRecordCreator
{
    Task<StudentCreatedCredential> AddAsync(StudentRegistrationInput input, int changedBy, CancellationToken ct = default);
}
