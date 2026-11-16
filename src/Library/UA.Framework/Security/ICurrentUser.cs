namespace UA.Framework.Security;
public interface ICurrentUser { int? UserId { get; } bool IsAuthenticated { get; } }
public sealed class SystemCurrentUser : ICurrentUser
{
    public int? UserId => null;
    public bool IsAuthenticated => false;
}
