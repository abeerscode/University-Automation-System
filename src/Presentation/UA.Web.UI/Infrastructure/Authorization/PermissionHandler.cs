using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UA.Facade.Interface.Administration;
namespace UA.Web.UI.Infrastructure.Authorization;
public sealed class PermissionHandler(IUserAccessFacade access) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            && await access.AllowedAsync(id, requirement.Code)) context.Succeed(requirement);
    }
}
