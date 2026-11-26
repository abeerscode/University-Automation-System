using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Student;
public interface IStudentStatusTransitionPolicy
{
    void EnsurePermitted(string existing, string next);
    bool EligibleForNewAcademicActivity(string status);
}
/// <summary>Normal staff actions only; drop, rejoin, graduation and alumni are workflow-owned in Phase 8.</summary>
public sealed class StudentStatusTransitionPolicy : IStudentStatusTransitionPolicy
{
    public bool EligibleForNewAcademicActivity(string status) => status == "Active";
    public void EnsurePermitted(string existing, string next)
    {
        if (existing == next) throw new BusinessRuleException("Status has not changed.");
        var permitted = existing switch
        {
            "Active" => next is "Suspended" or "Withdrawn" or "Expelled",
            "Suspended" => next is "Active" or "Withdrawn" or "Expelled",
            _ => false
        };
        if (!permitted)
            throw new BusinessRuleException("This transition is not permitted here. Semester drop, rejoin and graduation have separate approved workflows in a later phase.");
    }
}
