using UA.Facade.DTO.Student;
namespace UA.Facade.Interface.Integration;
public interface IStudentImportReader
{
    StudentImportSheet Read(byte[] data, string extension);
}
public interface IStudentPhotoStorage
{
    Task<string> SaveAsync(string studentId, byte[] data, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
}
