using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Student;
using UA.Entities;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Integration;
using UA.Facade.Interface.Student;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;
public sealed class StudentPortalFacade(IStudentRepository students, IRepository<UploadedDocument> documents,
    ICurrentUser current, IStudentPhotoStorage photos, IFileStorage files,
    IUnitOfWork work, IClock clock) : IStudentPortalFacade
{
    private async Task<UA.Entities.Student> OwnAsync(CancellationToken ct)
    {
        var actor = current.UserId ?? throw new ForbiddenException();
        var basic = await students.ByAccountAsync(actor, ct) ?? throw new ForbiddenException("Student account required.");
        return await students.GetTrackedAsync(basic.StudentId, ct) ?? throw new NotFoundException();
    }
    public async Task<StudentDetailDto> OwnProfileAsync(CancellationToken ct = default)
        => StudentDetailMapper.Map(await OwnAsync(ct));
    public async Task UpdateOwnAsync(string phone, string address, byte[]? photo,
        string? contentType, CancellationToken ct = default)
    {
        var x = await OwnAsync(ct);
        if (string.IsNullOrWhiteSpace(phone) || phone.Trim().Length > 20 ||
            string.IsNullOrWhiteSpace(address) || address.Trim().Length > 255)
            throw new BusinessRuleException("Phone and address are required (20/255 characters maximum).");
        string? created = null;
        if (photo is not null)
        {
            if (photo.Length is < 1 or > 1_048_576 || contentType is not ("image/jpeg" or "image/png"))
                throw new BusinessRuleException("Photo must be JPEG or PNG, at most 1 MB.");
            try { created = await photos.SaveAsync(x.StudentId, photo, contentType, ct); }
            catch (ArgumentException ex) { throw new BusinessRuleException(ex.Message); }
        }
        try
        {
            x.Phone = phone.Trim(); x.Address = address.Trim();
            if (created is not null)
            {
                x.PhotoPath = created;
                await documents.AddAsync(new UploadedDocument { StudentId = x.StudentId,
                    DocumentType = "Photo", FilePath = created, MimeType = contentType!,
                    FileSizeKb = (photo!.Length + 1023) / 1024, UploadedAt = clock.Now }, ct);
            }
            await work.SaveChangesAsync(ct);
        }
        catch
        {
            if (created is not null) await photos.DeleteAsync(created, CancellationToken.None);
            throw;
        }
    }
    public async Task<StudentPhotoDto> OwnPhotoAsync(CancellationToken ct = default)
    {
        var x = await OwnAsync(ct);
        if (string.IsNullOrEmpty(x.PhotoPath)) throw new NotFoundException("Student has not uploaded a photo.");
        var mime = x.PhotoPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
        return new StudentPhotoDto(await files.ReadAsync(x.PhotoPath, ct), mime);
    }
}
