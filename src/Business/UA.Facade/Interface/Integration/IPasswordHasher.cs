namespace UA.Facade.Interface.Integration;
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string storedHash); }
public interface ISecureTokenGenerator { string GenerateToken(); string GenerateCode(); string HashToken(string token); }
public interface IEmailSender { Task SendAsync(string destination, string subject, string body, CancellationToken ct = default); }
public interface IFileStorage { Task<byte[]> ReadAsync(string path, CancellationToken ct = default); }
public interface IExcelWriter { byte[] WriteCsv(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows); }
