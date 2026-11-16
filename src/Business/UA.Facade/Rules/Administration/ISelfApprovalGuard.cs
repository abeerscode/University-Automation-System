using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Administration;
public interface ISelfApprovalGuard { void Check(int actingUserId, int requestorId); }
public sealed class SelfApprovalGuard : ISelfApprovalGuard
{
    public void Check(int actingUserId, int requestorId)
    { if (actingUserId == requestorId) throw new BusinessRuleException("You cannot approve your own request."); }
}
