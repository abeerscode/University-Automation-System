namespace UA.Facade.Interface.Integration;
public interface IPrivateReportStorage { Task WriteReportAsync(int jobId,byte[] data,CancellationToken ct=default); Task<byte[]> ReadReportAsync(int jobId,CancellationToken ct=default); Task DeleteReportAsync(int jobId,CancellationToken ct=default); }
