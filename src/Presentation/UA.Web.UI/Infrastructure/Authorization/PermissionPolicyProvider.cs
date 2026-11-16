using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
namespace UA.Web.UI.Infrastructure.Authorization;
public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(HasPermissionAttribute.Prefix, StringComparison.Ordinal))
            return base.GetPolicyAsync(policyName);
        string code = policyName[HasPermissionAttribute.Prefix.Length..];
        return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(code)).Build());
    }
}
