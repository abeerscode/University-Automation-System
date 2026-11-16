using System.Security.Claims;
using UA.Framework.Security;
namespace UA.Web.UI.Infrastructure.Security;
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public int? UserId => int.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
