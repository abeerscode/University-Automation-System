namespace UA.Facade.Interface.Integration;
public interface IApplicantStorage
{
 Task<string> SaveApplicantAsync(byte[] data,string mime,CancellationToken ct=default);
 Task DeleteApplicantAsync(string path,CancellationToken ct=default);
}
