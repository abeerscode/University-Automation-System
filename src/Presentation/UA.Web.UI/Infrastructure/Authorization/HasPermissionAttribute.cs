using Microsoft.AspNetCore.Authorization;
namespace UA.Web.UI.Infrastructure.Authorization;
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string Prefix = "Permission:";
    public HasPermissionAttribute(string permission) => Policy = Prefix + permission;
}
