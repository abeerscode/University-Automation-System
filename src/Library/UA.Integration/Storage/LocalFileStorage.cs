using Microsoft.Extensions.Configuration;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Storage;
public sealed class LocalFileStorage(IConfiguration config) : IFileStorage, IStudentPhotoStorage, IFacultyPhotoStorage, IApplicantStorage, IPrivateReportStorage
{
    // Preserve an explicit PrivateRoot when configured. For fresh Phase 3
    // deployments, use a private per-service-user location without altering
    // existing appsettings files or exposing photos through wwwroot.
    private string StorageRoot()
    {
        var configured = config["FileStorage:PrivateRoot"];
        if (!string.IsNullOrWhiteSpace(configured)) return Path.GetFullPath(configured);
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local)) local = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(local, "UniversityAutomation", "PrivateFiles"));
    }
    public async Task<byte[]> ReadAsync(string path, CancellationToken ct = default)
    {
        var absoluteRoot = StorageRoot();
        var absolutePath = Path.GetFullPath(Path.Combine(absoluteRoot, path));
        if (!absolutePath.StartsWith(absoluteRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid document path.");
        return await File.ReadAllBytesAsync(absolutePath, ct);
    }
    public async Task<string> SaveAsync(string studentId, byte[] data, string contentType, CancellationToken ct = default)
    {
        if (studentId.Length != 8 || !studentId.All(char.IsDigit) || data.Length is < 1 or > 1_048_576)
            throw new ArgumentException("Invalid student ID or photo file size.");
        string extension;
        if (contentType == "image/png" && data.Length >= 8 &&
            data.AsSpan(0,8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})) extension = ".png";
        else if (contentType == "image/jpeg" && data.Length >= 3 && data[0] == 0xff && data[1] == 0xd8 && data[2] == 0xff)
            extension = ".jpg";
        else throw new ArgumentException("Use a valid PNG or JPEG photo.");
        string folder = Path.Combine(StorageRoot(), "student-photos");
        Directory.CreateDirectory(folder);
        string name = $"{studentId}-{Guid.NewGuid():N}{extension}";
        string path = Path.Combine(folder, name);
        await File.WriteAllBytesAsync(path, data, ct);
        return "student-photos/" + name;
    }
    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var root = StorageRoot();
        var allowed = Path.GetFullPath(Path.Combine(root, "student-photos")) + Path.DirectorySeparatorChar;
        var absolute = Path.GetFullPath(Path.Combine(root, path));
        if (absolute.StartsWith(allowed, StringComparison.Ordinal) && File.Exists(absolute)) File.Delete(absolute);
        return Task.CompletedTask;
    }

    public async Task<string> SaveFacultyAsync(string employeeRef, byte[] data, string contentType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(employeeRef) || employeeRef.Length > 30 ||
            !employeeRef.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ||
            data.Length is < 1 or > 1_048_576)
            throw new ArgumentException("Invalid faculty reference or photo size.");
        string extension;
        if (contentType == "image/png" && data.Length >= 8 &&
            data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) extension = ".png";
        else if (contentType == "image/jpeg" && data.Length >= 3 &&
            data[0] == 0xff && data[1] == 0xd8 && data[2] == 0xff) extension = ".jpg";
        else throw new ArgumentException("Use a genuine JPEG or PNG image no larger than 1 MB.");
        var folder = Path.Combine(StorageRoot(), "faculty-photos");
        Directory.CreateDirectory(folder);
        var fileName = $"{employeeRef}-{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(folder, fileName), data, ct);
        return "faculty-photos/" + fileName;
    }
    public Task DeleteFacultyAsync(string path, CancellationToken ct = default)
    {
        var root = StorageRoot();
        var allowed = Path.GetFullPath(Path.Combine(root, "faculty-photos")) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(root, path));
        if (full.StartsWith(allowed, StringComparison.Ordinal) && File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }
    public async Task<string> SaveApplicantAsync(byte[] data,string mime,CancellationToken ct=default)
    {
        string ext = mime switch {
            "image/png" when data.Length>=8 && data.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}) => ".png",
            "image/jpeg" when data.Length>=3 && data[0]==255 && data[1]==216 && data[2]==255 => ".jpg",
            "application/pdf" when data.Length>=5 && data.AsSpan(0,5).SequenceEqual("%PDF-"u8) => ".pdf",
            _ => throw new UA.Framework.Exceptions.BusinessRuleException("Invalid photo or PDF file.") };
        if(data.Length> (mime=="application/pdf"?5_000_000:1_048_576)) throw new UA.Framework.Exceptions.BusinessRuleException("File exceeds size limit.");
        var folder=Path.Combine(StorageRoot(),"applicant-files"); Directory.CreateDirectory(folder);
        var name=Guid.NewGuid().ToString("N")+ext; await File.WriteAllBytesAsync(Path.Combine(folder,name),data,ct); return "applicant-files/"+name;
    }
    private string ReportPath(int id){if(id<=0)throw new ArgumentOutOfRangeException(nameof(id));return Path.Combine(StorageRoot(),"private-reports",id.ToString(System.Globalization.CultureInfo.InvariantCulture)+".bin");}
    public async Task WriteReportAsync(int jobId,byte[] data,CancellationToken ct=default){var path=ReportPath(jobId);Directory.CreateDirectory(Path.GetDirectoryName(path)!);var tmp=path+".tmp";try{await File.WriteAllBytesAsync(tmp,data,ct);File.Move(tmp,path,true);}finally{if(File.Exists(tmp))File.Delete(tmp);}}
    public Task<byte[]> ReadReportAsync(int jobId,CancellationToken ct=default)=>File.ReadAllBytesAsync(ReportPath(jobId),ct);
    public Task DeleteReportAsync(int jobId,CancellationToken ct=default){var path=ReportPath(jobId);if(File.Exists(path))File.Delete(path);if(File.Exists(path+".tmp"))File.Delete(path+".tmp");return Task.CompletedTask;}
    public Task DeleteApplicantAsync(string path,CancellationToken ct=default)
    {
        var root=StorageRoot(); var folder=Path.GetFullPath(Path.Combine(root,"applicant-files"))+Path.DirectorySeparatorChar;
        var full=Path.GetFullPath(Path.Combine(root,path)); if(full.StartsWith(folder,StringComparison.Ordinal)&&File.Exists(full))File.Delete(full); return Task.CompletedTask;
    }
}
