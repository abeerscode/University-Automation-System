using Microsoft.Extensions.Logging;
using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Facade.Rules.Administration;
using UA.Framework.Common;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class AuthFacade(IUserAccountRepository users, IVerificationTokenRepository tokens,
    IUnitOfWork work, IPasswordHasher passwords, ISecureTokenGenerator random,
    IPasswordPolicy policy, IClock clock, IEmailSender email, ILogger<AuthFacade> logger) : IAuthFacade
{
    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await users.FirstAsync(u => u.Username == username, ct);
        // Never tell the caller whether a username exists.
        if (user is null) { passwords.Verify(password, passwords.Hash("constant-time dummy")); return new(false, "Invalid credentials."); }
        if (user.Status != "Active") return new(false, "Account is unavailable.");
        if (user.LockedUntil is DateTime until && until > clock.Now) return new(false, "Account temporarily locked.", Locked: true);
        if (!passwords.Verify(password, user.PasswordHash))
        {
            user.FailedLoginCount = (short)Math.Min(short.MaxValue, user.FailedLoginCount + 1);
            if (user.FailedLoginCount >= 5) { user.LockedUntil = clock.Now.AddMinutes(15); user.FailedLoginCount = 0; }
            await work.SaveChangesAsync(ct);
            return new(false, user.LockedUntil > clock.Now ? "Account temporarily locked." : "Invalid credentials.", Locked: user.LockedUntil > clock.Now);
        }
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = clock.Now;
        await work.SaveChangesAsync(ct);
        return new(true, "Signed in.", user.UserId, user.Username);
    }
    public async Task RequestResetAsync(string username, string origin, CancellationToken ct = default)
    {
        var user = await users.FirstAsync(u => u.Username == username, ct);
        if (user is null || user.Status != "Active" || string.IsNullOrEmpty(user.Email)) return;
        var token = random.GenerateToken();
        await tokens.AddAsync(new VerificationToken { UserId = user.UserId, Purpose = "Reset",
            CodeHash = random.HashToken(token), ExpiresAt = clock.Now.AddMinutes(30) }, ct);
        await work.SaveChangesAsync(ct);
        var url = origin.TrimEnd('/') + "/Account/ResetPassword?username=" + Uri.EscapeDataString(user.Username) + "&token=" + Uri.EscapeDataString(token);
        try { await email.SendAsync(user.Email, "UAS password reset", "Use this reset link within 30 minutes: " + url, ct); }
        catch (Exception ex) { logger.LogError(ex, "Password reset delivery failed"); }
    }
    public async Task<OperationResult> ResetPasswordAsync(string username, string token, string password, CancellationToken ct = default)
    {
        var issue = policy.Validate(password);
        if (issue is not null) return OperationResult.Fail(issue);
        var user = await users.FirstAsync(x => x.Username == username && x.Status == "Active", ct);
        if (user is null) return OperationResult.Fail("Invalid or expired reset link.");
        var digest = random.HashToken(token);
        var match = await tokens.FirstAsync(x => x.UserId == user.UserId && x.Purpose == "Reset" &&
            x.CodeHash == digest && x.UsedAt == null && x.ExpiresAt >= clock.Now, ct);
        if (match is null) return OperationResult.Fail("Invalid or expired reset link.");
        user.PasswordHash = passwords.Hash(password); user.FailedLoginCount = 0; user.LockedUntil = null;
        match.UsedAt = clock.Now;
        await work.SaveChangesAsync(ct);
        return OperationResult.Ok("Password changed. Please sign in.");
    }
    public async Task<OperationResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await users.FirstAsync(x => x.UserId == userId && x.Status == "Active", ct);
        if (user is null || !passwords.Verify(oldPassword, user.PasswordHash)) return OperationResult.Fail("Current password is incorrect.");
        var issue = policy.Validate(newPassword);
        if (issue is not null) return OperationResult.Fail(issue);
        user.PasswordHash = passwords.Hash(newPassword);
        await work.SaveChangesAsync(ct);
        return OperationResult.Ok("Password updated.");
    }
    public async Task RequestVerificationAsync(int userId, CancellationToken ct = default)
    {
        var user = await users.FirstAsync(x => x.UserId == userId && x.Status == "Active", ct);
        if (user is null || string.IsNullOrWhiteSpace(user.Email) || user.IsVerified) return;
        var code = random.GenerateCode();
        await tokens.AddAsync(new VerificationToken { UserId = user.UserId, Purpose = "Verify",
            CodeHash = random.HashToken(code), ExpiresAt = clock.Now.AddMinutes(10) }, ct);
        await work.SaveChangesAsync(ct);
        await email.SendAsync(user.Email, "UAS verification code", $"Your verification code is {code}. It expires in 10 minutes.", ct);
    }
    public async Task<OperationResult> VerifyCodeAsync(int userId, string code, CancellationToken ct = default)
    {
        if (code.Length != 6 || !code.All(char.IsDigit)) return OperationResult.Fail("Invalid code.");
        var list = await tokens.ListAsync(x => x.UserId == userId && x.Purpose == "Verify" && x.UsedAt == null, ct);
        var latest = list.OrderByDescending(x => x.TokenId).FirstOrDefault();
        if (latest is null || latest.ExpiresAt < clock.Now || latest.Attempts >= 5)
            return OperationResult.Fail("Code expired or attempts exhausted.");
        if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(random.HashToken(code)), System.Text.Encoding.UTF8.GetBytes(latest.CodeHash)))
        {
            latest.Attempts++; await work.SaveChangesAsync(ct);
            return OperationResult.Fail("Invalid code.");
        }
        var user = await users.FirstAsync(x => x.UserId == userId, ct);
        if (user is null) return OperationResult.Fail("Account not found.");
        latest.UsedAt = clock.Now; user.IsVerified = true;
        await work.SaveChangesAsync(ct);
        return OperationResult.Ok("Email verified.");
    }
}
