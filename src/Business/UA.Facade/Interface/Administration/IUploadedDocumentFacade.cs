namespace UA.Facade.Interface.Administration;
public interface IUploadedDocumentFacade
{
    Task<(string FileName, string ContentType, byte[] Data)> ReadAsync(int documentId, CancellationToken ct = default);
}
