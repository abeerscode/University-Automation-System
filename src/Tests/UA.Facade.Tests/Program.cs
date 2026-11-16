using UA.Facade.Rules.Administration;
using UA.Integration.Security;
using UA.Framework.Exceptions;
using UA.Framework.Constants;
static void Check(bool assertion, string name)
{
    if (!assertion) throw new Exception("FAILED: " + name);
    Console.WriteLine("PASS " + name);
}
var policy = new PasswordPolicy();
Check(policy.Validate("abc") is not null, "Reject short passwords");
Check(policy.Validate("password123A") is not null, "Reject common passwords");
Check(policy.Validate("validComplex123A") is null, "Accept strong passwords");
var hasher = new Pbkdf2PasswordHasher();
string hash = hasher.Hash("validComplex123A");
Check(hasher.Verify("validComplex123A", hash), "Verify password hashes");
Check(!hasher.Verify("wrongPass123A", hash), "Reject invalid passwords");
Check(hasher.Hash("validComplex123A") != hash, "Generate unique salt per password");
var tokens = new SecureTokenGenerator();
Check(tokens.GenerateToken() != tokens.GenerateToken(), "Secure random reset tokens");
Check(tokens.GenerateCode().Length == 6, "Six-digit verification codes");
Check(tokens.HashToken("sample") == tokens.HashToken("sample"), "Deterministic token hashing");
var guard = new SelfApprovalGuard();
bool blocked = false; try { guard.Check(17, 17); } catch (BusinessRuleException) { blocked = true; }
Check(blocked, "Prevent self-approval");
Check(PermissionCodes.All.Distinct().Count() == PermissionCodes.All.Length, "No duplicate permission codes");
Console.WriteLine("Phase-1 rule checks passed.");
