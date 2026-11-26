namespace UA.Facade.Interface.Integration;
public interface IFacultyPhotoStorage
{
    Task<string> SaveFacultyAsync(string employeeRef, byte[] data, string contentType, CancellationToken ct = default);
    Task DeleteFacultyAsync(string path, CancellationToken ct = default);
}
