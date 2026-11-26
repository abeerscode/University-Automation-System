namespace UA.Facade.Rules.Faculty;
public interface IWorkloadCalculator { decimal Calculate(IEnumerable<decimal> assignedCredits); }
/// <summary>Each active offering assignment contributes its course credits once for this teacher.</summary>
public sealed class WorkloadCalculator : IWorkloadCalculator
{
    public decimal Calculate(IEnumerable<decimal> assignedCredits) => assignedCredits.Sum();
}
