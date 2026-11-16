namespace UA.Facade.Rules.Administration;
public interface IPasswordPolicy { string? Validate(string password); }
public sealed class PasswordPolicy : IPasswordPolicy
{
    private static readonly HashSet<string> Blocked = new(StringComparer.OrdinalIgnoreCase)
        { "password123", "password1234", "qwerty12345", "1234567890", "admin123456", "welcome1234", "letmein1234", "password123!" };
    public string? Validate(string password)
    {
        if (password.Length < 10) return "Password needs at least 10 characters.";
        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            return "Use uppercase, lowercase and a number.";
        if (Blocked.Contains(password) || password.Contains("password", StringComparison.OrdinalIgnoreCase))
            return "Choose a less common password.";
        return null;
    }
}
