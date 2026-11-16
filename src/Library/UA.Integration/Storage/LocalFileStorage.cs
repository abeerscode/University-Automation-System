using Microsoft.Extensions.Configuration;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Storage;
public sealed class LocalFileStorage(IConfiguration config) : IFileStorage
{
    public async Task<byte[]> ReadAsync(string path, CancellationToken ct = default)
    {
        var root = config["FileStorage:PrivateRoot"];
        if (string.IsNullOrWhiteSpace(root)) throw new InvalidOperationException("Private file storage is not configured.");
        var absoluteRoot = Path.GetFullPath(root);
        var absolutePath = Path.GetFullPath(Path.Combine(absoluteRoot, path));
        if (!absolutePath.StartsWith(absoluteRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid document path.");
        return await File.ReadAllBytesAsync(absolutePath, ct);
    }
}
