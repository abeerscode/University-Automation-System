using UA.DataAccess.Interface.Administration;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Framework.Exceptions;
namespace UA.Facade.Implementation.Administration;
public sealed class UploadedDocumentFacade(IUploadedDocumentRepository docs, IUserAccessFacade access,
    IFileStorage storage, IManualAuditService audit) : IUploadedDocumentFacade
{
    public async Task<(string FileName, string ContentType, byte[] Data)> ReadAsync(int documentId, CancellationToken ct = default)
    {
        // Until ownership checks for Student/Applicant are added, download is staff-admin-only.
        await access.RequireAsync("files.download", ct: ct);
        var document = await docs.FirstAsync(x => x.DocumentId == documentId, ct) ?? throw new NotFoundException();
        if (document.FileSizeKb > 20_000) throw new BusinessRuleException("Document exceeds the configured download limit.");
        var bytes = await storage.ReadAsync(document.FilePath, ct);
        await audit.LogAsync("Download", "UploadedDocument", documentId.ToString(), ct: ct);
        return ("document-" + documentId, document.MimeType, bytes);
    }
}
